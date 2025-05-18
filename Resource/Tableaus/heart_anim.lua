local Vector         = require("lua.vector")
local sprites        = require("lua.all_sprites")
local ease           = require("lua.ease")
local frameAnimation = require("lua.frame_animation")
local elapsedTime    = 0


function ipsum.setup()
    ipsum:setWidth(50)
end

local image = ipsum:loadImage("hearts")
local topLeft = image:queryTagPositions("top_left")[1]
local frames = image:queryTagPositions("frame")


function ipsum.update(dt)
    elapsedTime = elapsedTime + dt
    frameAnimation.drawFrame(image, frames, 10, 10, 4, 4, elapsedTime * 5)
end
