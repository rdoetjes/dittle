APP=dittle
SBOM=$(APP)-sbom.json

.PHONY: sbom scan clean

release:
	dotnet build -c Release

sbom:
	syft . -o cyclonedx-json=$(SBOM)
	grype sbom:$(SBOM)

clean:
	rm -f $(SBOM)
