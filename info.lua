local buildDirectory = ".build"

local info = {
    appName = "LD57",
    itchUrl = "LD57",
    iconPath = "LD57/Icon.bmp",
    buildDirectory = buildDirectory,

    platformToProject =
    {
        ["macos-universal"] = "LD57",
        ["win-x64"] = "LD57",
        ["linux-x64"] = "LD57",
    },

    butlerChannelForPlatform = function(platform)
        return platform
    end,

    buildDirectoryForPlatform = function(platform)
        return buildDirectory .. '/' .. platform
    end
}

return info
