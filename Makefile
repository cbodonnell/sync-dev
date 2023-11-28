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

.PHONY: server-scenes
server-scenes:
	echo "Building Server scenes..."
	cp ./Sync_Client/Scenes/World/World.tscn ./Sync_Server/Scenes/World/World.tscn
	cp ./Sync_Client/Scenes/Player/Player.tscn ./Sync_Server/Scenes/Player/Player.tscn
