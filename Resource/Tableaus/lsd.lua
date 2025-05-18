local Vector      = require("lua.vector")
local sprites     = require("lua.all_sprites")
local ease        = require("lua.ease")
local elapsedTime = 0

function ipsum.setup()
    ipsum:setWidth(40)
end

local noise = ipsum:noise()
local colors = ipsum:allColors()

local intensity = 0

function ipsum.update(dt)
    elapsedTime = elapsedTime + dt

    intensity = ease.quadSlowFast(elapsedTime)

    local pixelIndex = 0
    for y = 0, ipsum:height() do
        for x = 0, ipsum:width() do
            pixelIndex = pixelIndex + 1
            local noiseIndex = elapsedTime * 2 + math.sin(x) + math.cos(y)
            if noise:number(pixelIndex) < intensity then
                local sprite = sprites[noise:integer(noiseIndex, #sprites) + 1]
                ipsum:setColor(colors[noise:integer(noiseIndex, #colors) + 1])
                ipsum:putTile(sprite, x, y)
            end
        end
    end
end
