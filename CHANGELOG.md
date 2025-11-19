# Changelog

All notable changes to FX.Spec will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.9.0-beta] - 2025-11-03

### Added
- Initial beta release of FX.Spec
- Core BDD DSL with `describe`, `context`, `it` functions
- Fluent assertion API with 50+ type-safe matchers
- Support for collections, strings, numbers, options, results, booleans, and exceptions
- Async test support with `itAsync`, `fitAsync`, `xitAsync`
- HTTP testing extensions with fluent matchers (`expectHttp`)
- Status code, header, and body matchers for HTTP responses
- TestServer integration for API testing
- Lifecycle hooks: `beforeEach`, `afterEach`, `beforeAll`, `afterAll`
- Focused tests (`fit`, `fdescribe`) and pending tests (`xit`, `pending`)
- Test discovery via `[<Tests>]` attribute
- Beautiful console output with Spectre.Console
- Documentation formatter with hierarchical output
- Simple formatter for CI/CD environments
- Clickable stack traces with file links (VS Code compatible)
- Filtered stack traces showing only user code
- Diff visualization for assertion failures
- Test randomization with configurable seed
- Command-line test runner (`fxspec`)
- Comprehensive documentation site
- 71 self-hosted tests validating the framework

### Changed
- F# language version set to `latest` (stable) instead of `preview`
- Package naming follows FX.* convention (FX.Spec.Core, FX.Spec.Matchers, etc.)

### Infrastructure
- NuGet package metadata configured for all projects
- README includes installation instructions
- FX.Spec.Runner packaged as .NET global tool
- All projects target .NET 9.0

## Project Links

- **Repository**: https://github.com/dawsons-creek/fx-spec
- **Documentation**: https://dawsons-creek.github.io/fx-spec
- **Issue Tracker**: https://github.com/dawsons-creek/fx-spec/issues
- **Discussions**: https://github.com/dawsons-creek/fx-spec/discussions

---

## Version History

- **0.9.0-beta** (2025-11-03) - Initial beta release

## Release Notes

### 0.9.0-beta - Initial Beta Release

This is the first public beta release of FX.Spec! The framework is feature-complete and ready for early adopters to try out and provide feedback.

**What's Ready:**
- Complete BDD testing framework with elegant DSL
- Comprehensive assertion library
- Async and HTTP testing support
- Beautiful error reporting
- Self-hosting with 71 passing tests

**What We're Looking For:**
- Feedback on API ergonomics
- Real-world usage patterns
- Bug reports and edge cases
- Performance characteristics
- Documentation clarity

**How to Help:**
Try FX.Spec in your projects and let us know:
- What works well
- What's confusing
- What's missing
- What could be better

File issues at: https://github.com/dawsons-creek/fx-spec/issues
Start discussions at: https://github.com/dawsons-creek/fx-spec/discussions

**Roadmap to 1.0:**
- v0.9.0-beta (now) - Gather feedback from early adopters
- v0.9.0-rc (Q1 2025) - Stabilize based on feedback
- v1.0.0 (Q2 2025) - Stable release with proven API

Thank you for trying FX.Spec!
