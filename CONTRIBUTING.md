# Contributing to QueryQuest

Thank you for your interest in contributing to QueryQuest! We welcome contributions to curriculum challenges, SQL execution security enhancements, UI accessibility, and performance improvements.

## How to Contribute

1. **Fork the Repository**: Create a personal fork of QueryQuest.
2. **Create a Feature Branch**:
   ```bash
   git checkout -b feature/your-feature-name
   ```
3. **Run Tests Locally**:
   Ensure all automated unit, integration, and security tests pass:
   ```bash
   dotnet test QueryQuest.sln
   ```
4. **Follow Architecture Boundaries**:
   - Keep application metadata (`queryquest_metadata.db`) and educational execution (`sample_educational.db`) strictly separated.
   - Do not perform mutation statements against learner databases.
   - Validate challenge solutions using data outputs rather than SQL string matching.
5. **Submit a Pull Request**:
   Open a pull request describing the change, testing steps, and any new challenge definitions.
