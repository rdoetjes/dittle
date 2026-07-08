APP=dittle
SBOM=$(APP)-sbom.json

.PHONY: sbom scan clean dist

release:
	dotnet publish -c Release -r win-x64 --self-contained true
	dotnet publish -c Release -r osx-arm64 --self-contained true
	dotnet publish -c Release -r osx-x64 --self-contained true
	dotnet publish -c Release -r linux-x64 --self-contained true

sbom:
	syft . -o cyclonedx-json=$(SBOM)
	grype sbom:$(SBOM)

dist:
	bash package.sh
