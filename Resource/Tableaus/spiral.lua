local Vector      = require("lua.vector")
local sprites     = require("lua.all_sprites")
local ease        = require("lua.ease")
local elapsedTime = 0


local center

function ipsum.setup()
    ipsum:setWidth(40)
    center = Vector.new(ipsum:width(), ipsum:height()) / 2
end

local function spiral(radius, spiralOffset, imageOffset)
    local t = -elapsedTime * 5
    ipsum.setColor(ipsum:allColors()[math.floor(radius / 2) + 1])
    ipsum.putTile(sprites[math.floor(radius + 1 + (imageOffset or 0))],
        center.x + (math.cos(t + radius / 10 + spiralOffset)) * radius,
        center.y + (math.sin(t + radius / 12 + spiralOffset)) * radius
    )
end

function ipsum.update(dt)
    local size = ipsum:width() / 2
    elapsedTime = elapsedTime + dt

    ipsum:setColor("green_text")

    local maxRadius = 20
    for radius = 0, maxRadius, 0.15 do
        spiral(radius, 0, 0)
        spiral(radius, math.pi)
    end
end
