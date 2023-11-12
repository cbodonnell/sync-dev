FROM alpine:latest

# update and install necessary packages, then download https://github.com/godotengine/godot/releases/download/4.1.3-stable/Godot_v4.1.3-stable_linux.x86_64.zip, unzip it, and move the binary to /usr/bin/godot

RUN apk update && apk add --no-cache \
    bash \
    curl \
    unzip \
    dotnet6-sdk


RUN curl -L "https://github.com/godotengine/godot/releases/download/4.1.3-stable/Godot_v4.1.3-stable_linux.x86_64.zip" -o godot.zip
RUN unzip godot.zip
RUN mv Godot_v4.1.3-stable_linux.x86_64 /usr/bin/godot
RUN rm godot.zip

# copy the project files to the container from the Sync_Server and the Shared folders
COPY ./Sync_Server /Sync_Server
COPY ./Shared /Shared

# the Shared folder is a dotnet project, so we need to build it
RUN cd /Shared && \
    dotnet build

# the Sync_Server folder is a godot project, so we need to build it
RUN cd /Sync_Server && \
    godot --headless --export-release "Linux/X11" "Sync_Server"

# allow execution of the server
RUN chmod +x "/Sync_Server/Build/Linux/Sync_Server"

# expose the port that the server will be running on
EXPOSE 9999

# run the server
CMD ["/Sync_Server/Build/Linux/Sync_Server", "--headless"]
