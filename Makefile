VERSION ?= $(shell git rev-parse --short HEAD)
ifneq ($(shell git status --porcelain),)
	VERSION := $(VERSION)-dirty
endif

.PHONY: container
container:
	docker build -t cheebz/sync-server:$(VERSION) -f ./Deploy/Dockerfile .

.PHONY: push
push:
	docker push cheebz/sync-server:$(VERSION)
