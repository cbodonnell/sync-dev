FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

RUN apt-get update && apt-get install -y \
    bash \
    wget \
    unzip

ENV GODOT_VERSION=4.1.3-stable

RUN wget "https://github.com/godotengine/godot/releases/download/${GODOT_VERSION}/Godot_v${GODOT_VERSION}_mono_linux_x86_64.zip"
RUN unzip Godot_v${GODOT_VERSION}_mono_linux_x86_64.zip
ENV GODOT_PATH=/Godot_v${GODOT_VERSION}_mono_linux_x86_64/Godot_v${GODOT_VERSION}_mono_linux.x86_64
RUN rm Godot_v${GODOT_VERSION}_mono_linux_x86_64.zip

RUN wget "https://github.com/godotengine/godot/releases/download/${GODOT_VERSION}/Godot_v${GODOT_VERSION}_mono_export_templates.tpz"
RUN unzip Godot_v${GODOT_VERSION}_mono_export_templates.tpz
RUN mkdir -p $HOME/.local/share/godot/export_templates/4.1.3.stable.mono
RUN mv ./templates/* $HOME/.local/share/godot/export_templates/4.1.3.stable.mono/

COPY ./Sync_Server /Sync_Server
COPY ./Shared /Shared

RUN cd /Shared && \
    dotnet build

RUN mkdir -p /Sync_Server/Build/Linux

RUN cd /Sync_Server && \
    ${GODOT_PATH} --headless --export-release "Linux/X11" "/Sync_Server/Build/Linux/Server.sh"

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime

COPY --from=build /Sync_Server/Build/Linux /Sync_Server

CMD ["/Sync_Server/Server.sh", "--headless"]
