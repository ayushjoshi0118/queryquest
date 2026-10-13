/**
 * QueryQuest - Frontend Application Logic
 * Vanilla JavaScript (ES Module)
 */

class QueryQuestApp {
  constructor() {
    this.challenges = [];
    this.currentChallenge = null;
    this.currentHintIndex = 0;
    this.progressOverview = null;
    this.activeTheme = localStorage.getItem('queryquest_theme') || 'dark';

    this.init();
  }

  async init() {
    this.applyTheme(this.activeTheme);
    this.bindEvents();
    await this.loadChallenges();
    await this.refreshProgress();
    await this.loadPlaygroundTables();

    // Select first challenge by default
    if (this.challenges.length > 0) {
      this.selectChallenge(this.challenges[0].id);
    }
  }

  applyTheme(theme) {
    this.activeTheme = theme;
    document.documentElement.setAttribute('data-theme', theme);
    localStorage.setItem('queryquest_theme', theme);
    const themeIcon = document.getElementById('theme-icon');
    if (themeIcon) {
      themeIcon.textContent = theme === 'dark' ? '☀️' : '🌙';
    }
  }

  bindEvents() {
    // Theme toggle
    document.getElementById('theme-toggle-btn').addEventListener('click', () => {
      this.applyTheme(this.activeTheme === 'dark' ? 'light' : 'dark');
    });

    // Navigation links
    document.querySelectorAll('.nav-btn').forEach(btn => {
      btn.addEventListener('click', (e) => {
        const view = e.target.getAttribute('data-view');
        this.switchView(view);
      });
    });

    document.getElementById('brand-link').addEventListener('click', (e) => {
      e.preventDefault();
      this.switchView('dashboard');
    });

    document.getElementById('btn-continue-learning').addEventListener('click', () => {
      this.switchView('learn');
      const firstUncompleted = this.challenges.find(c => !c.isCompleted);
      if (firstUncompleted) {
        this.selectChallenge(firstUncompleted.id);
      }
    });

    // Search and filter
    document.getElementById('search-challenges').addEventListener('input', () => this.filterChallenges());
    document.getElementById('filter-category').addEventListener('change', () => this.filterChallenges());
    document.getElementById('filter-difficulty').addEventListener('change', () => this.filterChallenges());

    // Learn Workspace Actions
    document.getElementById('btn-reset-sql').addEventListener('click', () => {
      if (this.currentChallenge) {
        document.getElementById('sql-editor').value = this.currentChallenge.starterSql;
      }
    });

    document.getElementById('btn-show-hint').addEventListener('click', () => this.fetchNextHint());
    document.getElementById('btn-show-solution').addEventListener('click', () => this.showReferenceSolution());
    document.getElementById('btn-run-query').addEventListener('click', () => this.runQuery());
    document.getElementById('btn-submit-solution').addEventListener('click', () => this.submitSolution());

    // Modals
    document.getElementById('btn-close-solution').addEventListener('click', () => {
      document.getElementById('solution-modal').classList.remove('active');
    });

    document.getElementById('btn-copy-solution').addEventListener('click', () => {
      const solutionText = document.getElementById('solution-sql-text').textContent;
      document.getElementById('sql-editor').value = solutionText;
      document.getElementById('solution-modal').classList.remove('active');
    });

    document.getElementById('btn-open-reset-modal').addEventListener('click', () => {
      document.getElementById('reset-modal').classList.add('active');
    });

    document.getElementById('btn-cancel-reset').addEventListener('click', () => {
      document.getElementById('reset-modal').classList.remove('active');
    });

    document.getElementById('btn-confirm-reset').addEventListener('click', () => this.resetProgress());

    // Playground Actions
    document.getElementById('btn-playground-clear').addEventListener('click', () => {
      document.getElementById('playground-sql-editor').value = '';
    });

    document.getElementById('btn-playground-run').addEventListener('click', () => this.runPlaygroundQuery());
  }

  switchView(viewName) {
    document.querySelectorAll('.nav-btn').forEach(btn => {
      btn.classList.toggle('active', btn.getAttribute('data-view') === viewName);
    });

    document.querySelectorAll('.view-section').forEach(sec => {
      sec.classList.remove('active');
    });

    const targetSection = document.getElementById(`view-${viewName}`);
    if (targetSection) {
      targetSection.classList.add('active');
    }

    if (viewName === 'dashboard' || viewName === 'progress') {
      this.refreshProgress();
    }
  }

  async loadChallenges() {
    try {
      const res = await fetch('/api/challenges');
      if (!res.ok) throw new Error('Failed to load challenges');
      this.challenges = await res.json();
      this.renderChallengeList(this.challenges);
    } catch (err) {
      console.error('Error fetching challenges:', err);
    }
  }

  filterChallenges() {
    const search = document.getElementById('search-challenges').value.toLowerCase().trim();
    const cat = document.getElementById('filter-category').value;
    const diff = document.getElementById('filter-difficulty').value;

    const filtered = this.challenges.filter(c => {
      const matchSearch = c.title.toLowerCase().includes(search) || c.learningObjective.toLowerCase().includes(search);
      const matchCat = cat === 'ALL' || c.category === cat;
      const matchDiff = diff === 'ALL' || c.difficulty === diff;
      return matchSearch && matchCat && matchDiff;
    });

    this.renderChallengeList(filtered);
  }

  renderChallengeList(list) {
    const container = document.getElementById('challenge-list');
    container.innerHTML = '';

    list.forEach(c => {
      const li = document.createElement('li');
      li.className = `challenge-item ${this.currentChallenge?.id === c.id ? 'active' : ''}`;
      li.innerHTML = `
        <div class="ch-status-dot ${c.isCompleted ? 'completed' : ''}" title="${c.isCompleted ? 'Completed' : 'Not Completed'}"></div>
        <div class="ch-info">
          <div class="ch-title">${this.escapeHtml(c.title)}</div>
          <div class="ch-meta">
            <span class="badge badge-${c.difficulty.toLowerCase()}">${c.difficulty}</span>
            <span>${c.points} pts</span>
          </div>
        </div>
      `;
      li.addEventListener('click', () => this.selectChallenge(c.id));
      container.appendChild(li);
    });
  }

  async selectChallenge(id) {
    try {
      const res = await fetch(`/api/challenges/${id}`);
      if (!res.ok) throw new Error('Failed to load challenge details');
      const challenge = await res.json();
      this.currentChallenge = challenge;
      this.currentHintIndex = 0;

      // Update UI elements
      document.getElementById('ws-ch-title').textContent = challenge.title;
      document.getElementById('ws-ch-objective').textContent = challenge.learningObjective;
      document.getElementById('ws-ch-statement').textContent = challenge.problemStatement;
      
      const badge = document.getElementById('ws-difficulty-badge');
      badge.textContent = challenge.difficulty;
      badge.className = `badge badge-${challenge.difficulty.toLowerCase()}`;

      // Reset hint text
      document.getElementById('hint-count-text').textContent = `0/${challenge.totalHintsAvailable}`;
      document.getElementById('hint-display-container').style.display = 'none';

      // Hide feedback box
      document.getElementById('feedback-container').style.display = 'none';

      // Set editor sql: load last submitted if any, else starter sql
      const editor = document.getElementById('sql-editor');
      editor.value = challenge.lastSubmittedSql || challenge.starterSql;

      // Highlight in list
      document.querySelectorAll('.challenge-item').forEach(el => {
        el.classList.toggle('active', el.textContent.includes(challenge.title));
      });

      // Load challenge schema
      await this.loadChallengeSchema(id);
    } catch (err) {
      console.error('Error selecting challenge:', err);
    }
  }

  async loadChallengeSchema(id) {
    const viewer = document.getElementById('schema-viewer');
    viewer.innerHTML = '<div style="color: var(--text-muted); font-size: 0.85rem;">Loading schema...</div>';

    try {
      const res = await fetch(`/api/challenges/${id}/schema`);
      if (!res.ok) throw new Error('Failed to fetch schema');
      const schemas = await res.json();

      viewer.innerHTML = '';
      schemas.forEach(s => {
        const block = document.createElement('div');
        block.className = 'schema-table-block';
        block.innerHTML = `
          <div class="schema-table-name">TABLE: ${s.tableName}</div>
          <div class="schema-col-list">
            ${s.columns.map(c => `
              <span class="schema-col-chip" title="Type: ${c.dataType} | PK: ${c.isPrimaryKey}">
                ${c.isPrimaryKey ? '🔑 ' : ''}${c.name} (${c.dataType})
              </span>
            `).join('')}
          </div>
        `;
        viewer.appendChild(block);
      });
    } catch (err) {
      viewer.innerHTML = '<div style="color: var(--accent-danger);">Error loading schema.</div>';
    }
  }

  async runQuery() {
    const sql = document.getElementById('sql-editor').value;
    const btn = document.getElementById('btn-run-query');
    btn.disabled = true;
    btn.textContent = 'Running...';

    try {
      const res = await fetch('/api/query/execute', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ sql })
      });

      const data = await res.json();
      this.displayResults(data, 'query-table-container', 'results-count-text', 'results-time-text');

      const fb = document.getElementById('feedback-container');
      const fbBox = document.getElementById('feedback-box');
      fb.style.display = 'block';

      if (data.success) {
        fbBox.className = 'feedback-box feedback-info';
        document.getElementById('feedback-message').textContent = `Query executed successfully (${data.rowCount} rows returned). Click 'Submit Answer' to test your solution!`;
        document.getElementById('feedback-explanation').textContent = '';
      } else {
        fbBox.className = 'feedback-box feedback-error';
        document.getElementById('feedback-message').textContent = `Query Error: ${data.error}`;
        document.getElementById('feedback-explanation').textContent = '';
      }
    } catch (err) {
      this.displayError('Failed to communicate with execution server.', 'feedback-container', 'feedback-box');
    } finally {
      btn.disabled = false;
      btn.textContent = '▶ Run Query';
    }
  }

  async submitSolution() {
    if (!this.currentChallenge) return;
    const sql = document.getElementById('sql-editor').value;
    const btn = document.getElementById('btn-submit-solution');
    btn.disabled = true;
    btn.textContent = 'Evaluating...';

    try {
      const res = await fetch(`/api/challenges/${this.currentChallenge.id}/submit`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ sql })
      });

      const data = await res.json();
      const fb = document.getElementById('feedback-container');
      const fbBox = document.getElementById('feedback-box');
      fb.style.display = 'block';

      if (data.isCorrect) {
        fbBox.className = 'feedback-box feedback-success';
        const pointsMsg = data.isFirstCompletion ? ` 🎉 +${data.pointsEarned} Quest Points!` : ' (Already completed)';
        document.getElementById('feedback-message').textContent = `${data.message}${pointsMsg}`;
        document.getElementById('feedback-explanation').textContent = data.conceptExplanation || '';

        // Display user results
        if (data.userResult) {
          this.displayResults(data.userResult, 'query-table-container', 'results-count-text', 'results-time-text');
        }

        // Refresh challenges and progress
        await this.loadChallenges();
        await this.refreshProgress();
      } else {
        fbBox.className = 'feedback-box feedback-error';
        document.getElementById('feedback-message').textContent = data.message;
        document.getElementById('feedback-explanation').textContent = data.conceptExplanation || '';

        if (data.userResult) {
          this.displayResults(data.userResult, 'query-table-container', 'results-count-text', 'results-time-text');
        }
      }
    } catch (err) {
      this.displayError('Error submitting solution to validation server.', 'feedback-container', 'feedback-box');
    } finally {
      btn.disabled = false;
      btn.textContent = '✓ Submit Answer';
    }
  }

  displayResults(data, containerId, countId, timeId) {
    const container = document.getElementById(containerId);
    const countEl = document.getElementById(countId);
    const timeEl = document.getElementById(timeId);

    if (countEl) countEl.textContent = `${data.rowCount || 0} rows ${data.isTruncated ? '(capped at 200)' : ''}`;
    if (timeEl) timeEl.textContent = `${data.executionDurationMs || 0} ms`;

    if (!data.success) {
      container.innerHTML = `
        <div class="empty-state" style="color: var(--accent-danger);">
          <div class="empty-state-icon">⚠️</div>
          <div>${this.escapeHtml(data.error || 'Execution failed')}</div>
        </div>
      `;
      return;
    }

    if (!data.rows || data.rows.length === 0) {
      container.innerHTML = `
        <div class="empty-state">
          <div class="empty-state-icon">📭</div>
          <div>Query executed successfully but returned 0 rows.</div>
        </div>
      `;
      return;
    }

    let html = '<table class="query-table"><thead><tr>';
    data.columns.forEach(col => {
      html += `<th>${this.escapeHtml(col)}</th>`;
    });
    html += '</tr></thead><tbody>';

    data.rows.forEach(row => {
      html += '<tr>';
      row.forEach(val => {
        if (val === null || val === undefined) {
          html += '<td class="null-val">NULL</td>';
        } else {
          html += `<td>${this.escapeHtml(val.toString())}</td>`;
        }
      });
      html += '</tr>';
    });

    html += '</tbody></table>';
    container.innerHTML = html;
  }

  async fetchNextHint() {
    if (!this.currentChallenge) return;
    try {
      const res = await fetch(`/api/challenges/${this.currentChallenge.id}/hint?hintIndex=${this.currentHintIndex}`);
      if (!res.ok) throw new Error('Failed to fetch hint');
      const hint = await res.json();

      const container = document.getElementById('hint-display-container');
      container.style.display = 'block';
      document.getElementById('hint-header-title').textContent = `💡 Hint ${hint.hintIndex + 1} of ${hint.totalHints}`;
      document.getElementById('hint-body-text').textContent = hint.hintText;

      document.getElementById('hint-count-text').textContent = `${hint.hintIndex + 1}/${hint.totalHints}`;
      if (hint.hasMoreHints) {
        this.currentHintIndex = hint.hintIndex + 1;
      }
    } catch (err) {
      console.error('Error fetching hint:', err);
    }
  }

  async showReferenceSolution() {
    if (!this.currentChallenge) return;
    try {
      const res = await fetch(`/api/challenges/${this.currentChallenge.id}/solution`);
      if (!res.ok) throw new Error('Failed to load solution');
      const data = await res.json();

      document.getElementById('solution-sql-text').textContent = data.referenceSolution;
      document.getElementById('solution-explanation-text').textContent = data.explanation;
      document.getElementById('solution-modal').classList.add('active');
    } catch (err) {
      console.error('Error loading solution:', err);
    }
  }

  async refreshProgress() {
    try {
      const res = await fetch('/api/progress');
      if (!res.ok) throw new Error('Failed to fetch progress');
      const p = await res.json();
      this.progressOverview = p;

      // Update Nav
      document.getElementById('nav-progress-text').textContent = `${p.completedChallenges} / ${p.totalChallenges}`;
      document.getElementById('nav-progress-fill').style.width = `${p.completionPercentage}%`;

      // Update Dashboard metrics
      document.getElementById('dash-completed').textContent = p.completedChallenges;
      document.getElementById('dash-completed-sub').textContent = `${p.completionPercentage}% of ${p.totalChallenges} challenges`;
      document.getElementById('dash-accuracy').textContent = `${p.accuracyRate}%`;
      document.getElementById('dash-accuracy-sub').textContent = `${p.totalSubmissions} submissions (${p.successfulSubmissions} correct)`;
      document.getElementById('dash-points').textContent = p.totalPoints;
      document.getElementById('dash-streak').textContent = `${p.currentStreakDays}d`;

      // Update Dashboard categories
      const catContainer = document.getElementById('dash-category-list');
      const progCatContainer = document.getElementById('progress-category-list');
      
      const renderCategoriesHtml = p.categories.map(cat => {
        const pct = cat.total > 0 ? Math.round((cat.completed / cat.total) * 100) : 0;
        return `
          <div class="cat-item">
            <div class="cat-info">
              <span>${this.escapeHtml(cat.category)}</span>
              <span style="font-family: var(--font-mono);">${cat.completed} / ${cat.total} (${pct}%)</span>
            </div>
            <div class="cat-bar-bg">
              <div class="cat-bar-fill" style="width: ${pct}%;"></div>
            </div>
          </div>
        `;
      }).join('');

      if (catContainer) catContainer.innerHTML = renderCategoriesHtml;
      if (progCatContainer) progCatContainer.innerHTML = renderCategoriesHtml;

      // Update Recent Activity
      const actContainer = document.getElementById('dash-recent-activity');
      if (p.recentActivity && p.recentActivity.length > 0) {
        actContainer.innerHTML = p.recentActivity.map(a => `
          <div style="padding: 0.65rem 0; border-bottom: 1px solid var(--border-subtle); display: flex; justify-content: space-between; align-items: center; font-size: 0.875rem;">
            <div>
              <div style="font-weight: 600;">${this.escapeHtml(a.challengeTitle)}</div>
              <div style="font-family: var(--font-mono); font-size: 0.75rem; color: var(--text-muted);">${this.escapeHtml(a.sqlPreview)}</div>
            </div>
            <span class="badge ${a.isCorrect ? 'badge-beginner' : 'badge-intermediate'}">${a.isCorrect ? 'Passed' : 'Attempted'}</span>
          </div>
        `).join('');
      }

      // Update Badges on Progress Page
      const badgesContainer = document.getElementById('badges-container');
      if (badgesContainer && p.badges) {
        badgesContainer.innerHTML = p.badges.map(b => `
          <div class="badge-card ${b.isUnlocked ? 'unlocked' : 'locked'}">
            <div class="badge-icon-box">${b.isUnlocked ? '🏆' : '🔒'}</div>
            <div>
              <div class="badge-title">${this.escapeHtml(b.title)}</div>
              <div class="badge-desc">${this.escapeHtml(b.description)}</div>
              ${b.isUnlocked ? `<div style="font-size: 0.7rem; color: var(--accent-success); margin-top: 0.25rem;">Unlocked!</div>` : ''}
            </div>
          </div>
        `).join('');
      }

      // Update Progress Page Activity Log Table
      const actTableContainer = document.getElementById('progress-activity-table-container');
      if (actTableContainer) {
        if (!p.recentActivity || p.recentActivity.length === 0) {
          actTableContainer.innerHTML = '<div style="color: var(--text-muted); padding: 1rem 0;">No submissions recorded yet.</div>';
        } else {
          actTableContainer.innerHTML = `
            <table class="query-table" style="width: 100%;">
              <thead>
                <tr>
                  <th>Time</th>
                  <th>Challenge</th>
                  <th>Submitted SQL Preview</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                ${p.recentActivity.map(a => `
                  <tr>
                    <td>${new Date(a.timestamp).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</td>
                    <td>${this.escapeHtml(a.challengeTitle)}</td>
                    <td style="font-family: var(--font-mono);">${this.escapeHtml(a.sqlPreview)}</td>
                    <td><span class="badge ${a.isCorrect ? 'badge-beginner' : 'badge-intermediate'}">${a.isCorrect ? 'Passed' : 'Failed'}</span></td>
                  </tr>
                `).join('')}
              </tbody>
            </table>
          `;
        }
      }

    } catch (err) {
      console.error('Error refreshing progress:', err);
    }
  }

  async resetProgress() {
    try {
      const res = await fetch('/api/progress/reset', { method: 'POST' });
      if (!res.ok) throw new Error('Failed to reset progress');

      document.getElementById('reset-modal').classList.remove('active');
      await this.loadChallenges();
      await this.refreshProgress();

      if (this.currentChallenge) {
        this.selectChallenge(this.currentChallenge.id);
      }
    } catch (err) {
      alert('Failed to reset progress. Please try again.');
    }
  }

  async loadPlaygroundTables() {
    try {
      const res = await fetch('/api/query/schema');
      if (!res.ok) throw new Error('Failed to load schemas');
      const tables = await res.json();

      const container = document.getElementById('playground-tables-list');
      container.innerHTML = tables.map(t => `
        <div style="margin-bottom: 1rem;">
          <div style="font-family: var(--font-mono); font-weight: 700; color: var(--accent-cyan); font-size: 0.9rem; cursor: pointer;" 
               onclick="document.getElementById('playground-sql-editor').value = 'SELECT * FROM ${t.tableName} LIMIT 10;'">
            📄 ${t.tableName}
          </div>
          <div style="font-size: 0.75rem; color: var(--text-secondary); margin-top: 0.25rem;">
            ${t.columns.map(c => c.name).join(', ')}
          </div>
        </div>
      `).join('');
    } catch (err) {
      console.error('Error loading playground tables:', err);
    }
  }

  async runPlaygroundQuery() {
    const sql = document.getElementById('playground-sql-editor').value;
    const btn = document.getElementById('btn-playground-run');
    btn.disabled = true;
    btn.textContent = 'Running...';

    try {
      const res = await fetch('/api/query/execute', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ sql })
      });

      const data = await res.json();
      this.displayResults(data, 'playground-table-container', 'playground-count-text', 'playground-time-text');
    } catch (err) {
      console.error('Error running playground query:', err);
    } finally {
      btn.disabled = false;
      btn.textContent = '▶ Run Query';
    }
  }

  displayError(msg, containerId, boxId) {
    const fb = document.getElementById(containerId);
    const box = document.getElementById(boxId);
    fb.style.display = 'block';
    box.className = 'feedback-box feedback-error';
    document.getElementById('feedback-message').textContent = msg;
    document.getElementById('feedback-explanation').textContent = '';
  }

  escapeHtml(str) {
    if (!str) return '';
    return str
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#039;');
  }
}

// Initialize on DOMContentLoaded
document.addEventListener('DOMContentLoaded', () => {
  window.app = new QueryQuestApp();
});
