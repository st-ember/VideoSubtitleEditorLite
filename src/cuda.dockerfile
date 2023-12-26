
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
RUN apt-get update -y && apt-get install -y fonts-dejavu-core
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["SubtitleEditor.Core/SubtitleEditor.Core.csproj", "SubtitleEditor.Core/"]
COPY ["SubtitleEditor.Database/SubtitleEditor.Database.csproj", "SubtitleEditor.Database/"]
COPY ["SubtitleEditor.Infrastructure/SubtitleEditor.Infrastructure.csproj", "SubtitleEditor.Infrastructure/"]
COPY ["SubtitleEditor.Web.Infrastructure/SubtitleEditor.Web.Infrastructure.csproj", "SubtitleEditor.Web.Infrastructure/"]
COPY ["SubtitleEditor.Worker.Infrastructure/SubtitleEditor.Worker.Infrastructure.csproj", "SubtitleEditor.Worker.Infrastructure/"]
COPY ["SubtitleEditor.Web/SubtitleEditor.Web.csproj", "SubtitleEditor.Web/"]
RUN dotnet restore "SubtitleEditor.Web/SubtitleEditor.Web.csproj"
COPY . .
WORKDIR "/src/SubtitleEditor.Web"
RUN dotnet build "SubtitleEditor.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SubtitleEditor.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false
RUN cp /app/publish/FFMpeg/ffmpeg /app/publish/
RUN cp /app/publish/FFMpeg/ffprobe /app/publish/
RUN rm -rf /app/publish/FFMpeg/

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SubtitleEditor.Web.dll"]