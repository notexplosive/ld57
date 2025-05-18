local Vector      = require("lua.vector")
local elapsedTime = 0


function ipsum.setup()
    ipsum:setWidth(60)
end

local image = ipsum.loadImage("jack")

local shapes = {}
local colors = {}
local backgrounds = {}
local indices = {}

for i, position in ipairs(image:positions()) do
    shapes[position] = image.getShapeAt(position.x, position.y)
    colors[position] = image.getColorAt(position.x, position.y)
    backgrounds[position] = {
        color = image.getBackgroundColorAt(position.x, position.y),
        intensity = image.getBackgroundIntensityAt(position.x, position.y)
    }
    indices[position] = i
end

local imageX = 18
local imageY = 5
local imagePosition = Vector.new(imageX, imageY)

local topIndex = 0

function ipsum.update(dt)
    elapsedTime = elapsedTime + dt

    for position, shape in pairs(shapes) do
        local finalTarget = Vector.new(position.x, position.y) + imagePosition
        local index = indices[position]


        if topIndex >= index then
            ipsum.setColor(colors[position], backgrounds[position].color, backgrounds[position].intensity)
            ipsum.putTile(shape, finalTarget.x, finalTarget.y)
        end
    end

    for position, shape in pairs(shapes) do
        local finalTarget = Vector.new(position.x, position.y) + imagePosition

        local index = indices[position]

        if math.abs(((elapsedTime * 500 - index))) < 15 then
            ipsum.setColor("green_text", "blue_brick", backgrounds[position].intensity)
            ipsum.putTile(shape, finalTarget.x, finalTarget.y + 1)
            topIndex = index
        end
    end
end
