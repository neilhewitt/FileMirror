# Contributing

## Welcome!

Thanks for considering contributing to FileMirror. This document describes how to contribute effectively.

## Code of Conduct

- Be respectful and inclusive
- Accept constructive criticism gracefully
- Focus on what's best for the community
- Show empathy towards other community members

## How to Contribute

### 1. Report Bugs

**Before reporting:**

- Check if bug is already reported
- Test with latest version
- Include version and steps to reproduce

**How to report:**

1. Open issue on GitHub
2. Include:
   - FileMirror version
   - Steps to reproduce
   - Expected behavior
   - Actual behavior
   - Error messages (sanitized)

### 2. Suggest Features

**Before suggesting:**

- Check if feature is already requested
- Read project goals (YAGNI, simplicity)

**How to suggest:**

1. Open issue on GitHub
2. Include:
   - Clear feature description
   - Use case
   - Examples (if applicable)
   - Alternatives considered

### 3. Submit Code

**Steps:**

1. Fork repository
2. Create feature branch
3. Make changes
4. Add tests
5. Run tests
6. Submit pull request

## Development Process

### 1. Set Up Development Environment

```bash
# Clone repository
git clone https://github.com/yourusername/FileMirror.git
cd FileMirror\src

# Build
dotnet build

# Run tests
dotnet test
```

### 2. Create Feature Branch

```bash
git checkout -b feature/your-feature-name
```

### 3. Make Changes

Follow coding style (see [Coding Style](coding-style.md)):

- No `var` (except anonymous types)
- Braces always
- No comments (unless requested)
- PascalCase naming
- Tests in NUnit

### 4. Write Tests

- Add tests for new code
- Update tests for changes
- Follow test naming convention
- Use Arrange, Act, Assert

### 5. Run Tests

```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "FullyQualifiedName~ConfigParserTests"
```

### 6. Build Release

```bash
dotnet build -c Release
dotnet publish -c Release --self-contained true /p:PublishSingleFile=true
```

### 7. Submit Pull Request

**PR template:**

```markdown
## Summary
- What changed
- Why it changed

## Testing
- Tests added/updated
- Manual testing performed

## Related Issues
Fixes #123
```

## Coding Standards

### Required

- Follow global AGENTS.md coding style
- Use NUnit (not xUnit)
- No mocking frameworks
- One class per file
- Filename matches class

### Recommended

- Expression-bodied members for simple methods
- Ternary operators for simple conditionals
- Pattern matching with switch expressions
- Readonly for immutable fields

## Pull Request Process

1. **Review**: Maintainers review PR
2. **Feedback**: Address feedback
3. **Merge**: Maintainers merge (or request changes)
4. **Deploy**: Released in next version

### PR Checklist

- [ ] Tests pass
- [ ] Code follows style guide
- [ ] No new warnings
- [ ] Documentation updated
- [ ] Changelog entry added

## Documentation

### Update Docs

Update relevant docs for changes:

- [docs/README.md](README.md)
- [docs/configuration.md](configuration.md)
- [docs/building.md](building.md)
- [docs/testing.md](testing.md)
- [docs/troubleshooting.md](troubleshooting.md)

### Add New Feature

1. Add to [docs/README.md](README.md)
2. Create new doc if needed
3. Link from main docs

## Community

### Channels

- **GitHub Issues**: Bugs, features
- **GitHub Discussions**: Questions, ideas

### Etiquette

- Be patient with reviews
- Accept feedback gracefully
- Credit contributors
- Help newcomers

## Recognition

Contributors:

- Listed in README
- Notified in release notes
- Mentioned in acknowledgments

## License

By contributing, you agree:

- Your contributions are licensed under the project license
- You have right to contribute
- Your contributions don't violate others' rights

## Questions?

- Check documentation
- Open issue for unclear requirements
- Join discussions for open-ended questions

---

**Thank you for contributing!**
