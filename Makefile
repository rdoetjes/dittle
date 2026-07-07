APP=dittle
SBOM=$(APP)-sbom.json

.PHONY: sbom scan clean

sbom:
	syft . -o cyclonedx-json=$(SBOM)
	grype sbom:$(SBOM)

clean:
	rm -f $(SBOM)
