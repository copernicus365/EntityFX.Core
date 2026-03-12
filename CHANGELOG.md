# Changelog

## [6.1.0] - 2026-03-12

### Fixed
- Added optional FROM keyword in all DELETE statements
  - Affected methods: DeleteAll(), DeleteDirect(), and related overloads
  
### Changed
- Refactored multiple methods to use expression-bodied members for cleaner code
- Improved exception handling with modern ArgumentException.ThrowIfNullOrEmpty() methods
- Updated ArgumentNullException to include parameter names

### Removed
- Removed test method from IDbRepository interface

### Details
See detailed changelog: [changelog-v6.1.0.md](./changelogs/changelog-v6.1.0.md)

---

## [6.0.2] - Previous Release

Previous changes...
