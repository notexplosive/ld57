local Vector      = require("lua.vector")
local sprites     = require("lua.all_sprites")
local ease        = require("lua.ease")
local elapsedTime = 0


function ipsum.setup()
    ipsum:setWidth(50)
end

local image = ipsum:loadImage("hearts")
local topLeft = image:queryTagPositions("top_left")[1]
local frames = image:queryTagPositions("frame")

local frameAnimation = {}

local frameCache = {}

function frameAnimation.drawFrame(image, x, y, w, h, t)
    if frameCache[image] == nil then
        frameCache[image] = image:queryTagPositions("frame")
    end

    local frames = frameCache[image]

    local frameIndex = math.floor(1 + elapsedTime * 5)
    if frameIndex > #frames then
        elapsedTime = 0
        frameIndex = 1
    end

    ipsum:putImageSlice(image, 1, 1, frames[frameIndex].x, frames[frameIndex].y, w, h)
end

function ipsum.update(dt)
    elapsedTime = elapsedTime + dt
    for i, tag in ipairs(frames) do
        ipsum:putText(tag.x .. "," .. tag.y, 0, i - 1)
    end

    local frameIndex = math.floor(1 + elapsedTime * 5)
    if frameIndex > #frames then
        elapsedTime = 0
        frameIndex = 1
    end
    ipsum:putImageSlice(image, 1, 1, frames[frameIndex].x, frames[frameIndex].y, 4, 4)
end
