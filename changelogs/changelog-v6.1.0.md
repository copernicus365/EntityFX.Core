# Change Log - 6.1.0 - March 12, 2026 (Claude analyzed)

## Summary of Changes in DbRepositoryCore and Related Files

### **DbRepositoryCore.cs** - Main Changes:

#### 1. **Fixed Missing `FROM` Keyword in DELETE Statements** ✅
   - All `DELETE` SQL statements now properly include `FROM` keyword

   (NOTE: this was NOT "improper" ... it's just preferred, I've learned, to have "DELETE FROM")

   - Changed: `DELETE {FullTableName}` → `DELETE FROM {FullTableName}`
   - Affected methods:
     - `DeleteAll()`
     - `_DeleteDirectStr(TId id)`
     - `DeleteDirect(NameValueMatch nvm)`
     - `_DeleteDirectStr(TId id, NameValueMatch nvm)`

#### 2. **Code Refactoring - Expression-Bodied Members** ✅
   - Converted several methods to use expression-bodied syntax (`=>`) for cleaner code:
     - `DeleteDirect(TId id)`
     - `DeleteDirectAsync(TId id)`
     - `DeleteAll()`
     - `DeleteDirectWhere(...)`
     - `DeleteDirectWhereAsync(...)`
     - `_DeleteDirectStr(TId id)`
     - `_DeleteDirectStr(TId id, NameValueMatch nvm)`
     - `DeleteDirect(NameValueMatch nvm)`

#### 3. **Improved Exception Handling** ✅
   - In `_DeleteDirectStrWhere()` method:
     - Replaced generic null checks with modern `ArgumentException.ThrowIfNullOrEmpty()`
     - Now properly throws exceptions with parameter names for `colName`, `operatr`, and `val`
   - In `__UpdateDirectHelper()`:
     - Changed `throw new ArgumentNullException()` to `throw new ArgumentNullException(nameof(args))`

### **IDbRepository.cs** - Interface Cleanup:

- **Removed Test Method** ✅
  - Removed `IQueryable<T> GetTest(TId xyz);` from the interface (was likely a test/debug method)

### **Project Files** - Version Updates:

- **EntityFX.Core.csproj**: Version bumped from `6.0.2` → `6.1.0`
- **EntityFX.Core.Base.csproj**: Version bumped from `6.0.2` → `6.1.0`

---

## Key Improvements:

✅ **SQL correctness** - Fixed invalid SQL syntax (missing `FROM` in DELETE statements)
✅ **Code quality** - Modernized to use expression-bodied members
✅ **Better error handling** - More descriptive exceptions with parameter names
✅ **API cleanup** - Removed test method from interface

These are solid improvements that enhance code quality, maintainability, and SQL correctness!
