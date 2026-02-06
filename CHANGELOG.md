## v1.0.6

No significant changes detected since v1.0.6.
## v1.0.6 (patch)

Changes since v1.0.5:

- Add target frameworks to UniversalSerializer project file ([@matt-edmondson](https://github.com/matt-edmondson))
## v1.0.5 (patch)

Changes since v1.0.4:

- refactor ([@matt-edmondson](https://github.com/matt-edmondson))
- Remove .github\workflows\project.yml ([@matt-edmondson](https://github.com/matt-edmondson))
## v1.0.5-pre.4 (prerelease)

Changes since v1.0.5-pre.3:

- Sync scripts\update-winget-manifests.ps1 ([@ktsu[bot]](https://github.com/ktsu[bot]))
## v1.0.5-pre.3 (prerelease)

Changes since v1.0.5-pre.2:

- Sync .github\workflows\dotnet.yml ([@ktsu[bot]](https://github.com/ktsu[bot]))
## v1.0.5-pre.2 (prerelease)

Changes since v1.0.5-pre.1:

- Sync .github\workflows\dotnet.yml ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Sync scripts\PSBuild.psm1 ([@ktsu[bot]](https://github.com/ktsu[bot]))
## v1.0.5-pre.1 (prerelease)

Incremental prerelease update.
## v1.0.4 (patch)

Changes since v1.0.3:

- Simplify dependency injection ([@matt-edmondson](https://github.com/matt-edmondson))
- Update project files and configurations ([@matt-edmondson](https://github.com/matt-edmondson))
- Add comprehensive unit tests to improve test coverage ([@matt-edmondson](https://github.com/matt-edmondson))
## v1.0.4-pre.2 (prerelease)

Changes since v1.0.4-pre.1:
## v1.0.4-pre.1 (prerelease)

Incremental prerelease update.
## v1.0.3 (patch)

Changes since v1.0.2:

- Enhance winget manifest update script with improved configuration and error handling ([@matt-edmondson](https://github.com/matt-edmondson))
## v1.0.2 (patch)

Changes since v1.0.1:

- Refactor Invoke-DotNetPack to use PackageReleaseNotesFile for changelog reference ([@matt-edmondson](https://github.com/matt-edmondson))
## v1.0.1 (patch)

Changes since v1.0.0:

- Enhance UniversalSerializer with SerializationProvider integration and new features ([@matt-edmondson](https://github.com/matt-edmondson))
- Refactor PSBuild script for improved functionality and clarity ([@matt-edmondson](https://github.com/matt-edmondson))
## v1.0.0 (major)

- Refactor project files to use custom SDKs for improved build management. Updated UniversalSerializer, UniversalSerializer.Benchmarks, and UniversalSerializer.Test projects to utilize ktsu.Sdk.Lib, ktsu.Sdk.ConsoleApp, and ktsu.Sdk.Test respectively. This change enhances compatibility and streamlines the build process across different project types. ([@matt-edmondson](https://github.com/matt-edmondson))
- Refine StringConvertibleTypeConverter and enhance documentation ([@matt-edmondson](https://github.com/matt-edmondson))
- Add compression and encryption support to serializers, including the implementation of ICompressionProvider and associated compression algorithms (GZip, Deflate). Integrated compression functionality into SerializerBase for byte serialization. Established security infrastructure with IEncryptionProvider and EncryptionType enum. Enhanced YAML polymorphic type converter and improved overall build stability by addressing compilation errors and validation issues. ([@matt-edmondson](https://github.com/matt-edmondson))
- Refactor SerializerServiceCollectionExtensions and update serializer registrations ([@matt-edmondson](https://github.com/matt-edmondson))
- Enhance serializer functionality and update dependencies ([@matt-edmondson](https://github.com/matt-edmondson))
- Add enum serialization format options to serializer settings ([@matt-edmondson](https://github.com/matt-edmondson))
- Add support for custom type conversion in serializers ([@matt-edmondson](https://github.com/matt-edmondson))
- Add support for MessagePack, Protobuf, and FlatBuffers serialization ([@matt-edmondson](https://github.com/matt-edmondson))
- Add YAML serialization support ([@matt-edmondson](https://github.com/matt-edmondson))
- Add initial project files for UniversalSerializer, including configuration for package management and serialization rules. Implemented core serializer interfaces and added YAML, JSON, and XML serializers with dependency injection support. Updated README for documentation and added initial tests for serializer functionality. ([@matt-edmondson](https://github.com/matt-edmondson))
- Add community and core plugins configuration; update project dependencies ([@matt-edmondson](https://github.com/matt-edmondson))
- Add support for MessagePack, Protobuf, and FlatBuffers serializers ([@matt-edmondson](https://github.com/matt-edmondson))
- Add core serialization framework with JSON and XML support ([@matt-edmondson](https://github.com/matt-edmondson))
- Enhance serializer options with format-specific keys and advanced customization ([@matt-edmondson](https://github.com/matt-edmondson))
- initial commit ([@matt-edmondson](https://github.com/matt-edmondson))
- Update .editorconfig to disable var usage for built-in types, modify .runsettings for parallel test execution, enhance .gitignore for SpecStory files, and update Directory.Packages.props with new dependencies and versioning. Introduce global.json for SDK management and refine PSBuild.psm1 for improved package publishing and version handling. ([@matt-edmondson](https://github.com/matt-edmondson))
- Update dependencies and enhance serialization options ([@matt-edmondson](https://github.com/matt-edmondson))
- Remove PowerShell script for adding missing using statements and update README to reflect package name change to ktsu.UniversalSerializer. Add a new rule for ensuring unit and integration tests with > 80% code coverage. ([@matt-edmondson](https://github.com/matt-edmondson))
- Remove unnescessary file ([@matt-edmondson](https://github.com/matt-edmondson))
- Complete implementation tasks for UniversalSerializer project ([@matt-edmondson](https://github.com/matt-edmondson))
- Add TOML serialization support ([@matt-edmondson](https://github.com/matt-edmondson))
- Add .cursorignore for SpecStory backup files, update Directory.Packages.props with main and test dependencies, and refactor project files to use Microsoft.NET.Sdk. Added new history file for library development progress and updated .gitignore to include backup files. ([@matt-edmondson](https://github.com/matt-edmondson))
- Update task list to mark build pipeline configuration as complete ([@matt-edmondson](https://github.com/matt-edmondson))
- Add INI serialization support ([@matt-edmondson](https://github.com/matt-edmondson))
- Add common and type registry option keys for serialization ([@matt-edmondson](https://github.com/matt-edmondson))
- Add streaming and compression support to serializers, including new methods in ISerializer for stream handling. Introduced CompressionType enum and updated SerializerOptions for compression settings. Enhanced TOML serializer with complete object conversion and added parameter validation for compliance. Fixed various compilation errors and improved YAML polymorphic type converter implementation. ([@matt-edmondson](https://github.com/matt-edmondson))
- Refactor SerializerRegistry and TomlSerializer for improved clarity and performance ([@matt-edmondson](https://github.com/matt-edmondson))
- Refactor serializer interfaces and implementations to improve code clarity and consistency. Removed unnecessary using directives, added access modifiers to interface members, and standardized variable declarations. Enhanced type handling in various serializers and converters, ensuring better adherence to coding standards and practices. ([@matt-edmondson](https://github.com/matt-edmondson))
- Remove Protobuf and FlatBuffers support from UniversalSerializer ([@matt-edmondson](https://github.com/matt-edmondson))
- Add polymorphic serialization support for JSON, XML, and YAML ([@matt-edmondson](https://github.com/matt-edmondson))
- Refactor SerializerRegistry and update serializer methods ([@matt-edmondson](https://github.com/matt-edmondson))
- Add XML serializer options, enhance JsonPolymorphicConverter for better type handling, and fix missing using statements across various serializers. Implemented HasPolymorphicTypes method in TypeRegistry and updated SerializerOptionKeys for XML settings. Addressed pattern matching issues in TOML and YAML serializers. ([@matt-edmondson](https://github.com/matt-edmondson))
- Update README.md with comprehensive documentation for UniversalSerializer ([@matt-edmondson](https://github.com/matt-edmondson))
- Add LZ4 compression support and enhance serializer error handling ([@matt-edmondson](https://github.com/matt-edmondson))
- Add implementation tasks for UniversalSerializer ([@matt-edmondson](https://github.com/matt-edmondson))
- Enhance serializer functionality and update project files ([@matt-edmondson](https://github.com/matt-edmondson))
- Add polymorphic serialization support with type discriminator options ([@matt-edmondson](https://github.com/matt-edmondson))
- Refactor SerializerRegistry and update serialization methods ([@matt-edmondson](https://github.com/matt-edmondson))
- Remove INI serialization support ([@matt-edmondson](https://github.com/matt-edmondson))
