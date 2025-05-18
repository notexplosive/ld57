local elapsedTime     = 0

local VectorMetaTable = {
}

local function createVector(x, y)
    local v = {
        x = x,
        y = y
    }
    setmetatable(v, VectorMetaTable)
    return v
end

VectorMetaTable["__index"] = VectorMetaTable
VectorMetaTable["type"] = "Vector"

VectorMetaTable["__add"] = function(a, b)
    return createVector(a.x + b.x, a.y + b.y)
end

VectorMetaTable["__sub"] = function(a, b)
    return createVector(a.x - b.x, a.y - b.y)
end

VectorMetaTable["__mul"] = function(a, b)
    local scaler = 0
    local v = createVector(0, 0)

    if tonumber(a) then
        scaler = tonumber(a)
        v = b
    end

    if tonumber(b) then
        scaler = tonumber(b)
        v = a
    end

    return createVector(v.x * scaler, v.y * scaler)
end

local function lerpVector(source, destination, percent)
    return source + (destination - source) * percent
end

function ipsum.setup()
    ipsum.width = 60
end

local function quadSlowFast(x)
    return x * x
end

local function quadFastSlow(x)
    return 1 - quadSlowFast(1 - x);
end

local noise = ipsum:noise()

local sprite = ipsum.sprite("Entities", 5)
local image = ipsum.loadImage("jack")

local shapes = {}
local colors = {}
local backgrounds = {}
local indices = {}
local times = {}
local totalIndex = 0

for i, position in ipairs(image:positions()) do
    shapes[position] = image.getShapeAt(position.x, position.y)
    colors[position] = image.getColorAt(position.x, position.y)
    backgrounds[position] = {
        color = image.getBackgroundColorAt(position.x, position.y),
        intensity = image.getBackgroundIntensityAt(position.x, position.y)
    }
    indices[position] = i
    times[position] = 0
    totalIndex = i
end

local imageX = 18
local imageY = 5
local imagePosition = createVector(imageX, imageY)
local center = createVector(image:width() / 2 + imagePosition.x, image:height() / 2 + imagePosition.y)

local topIndex = 0

function ipsum.update(dt)
    elapsedTime = elapsedTime + dt

    for position, shape in pairs(shapes) do
        local finalTarget = createVector(position.x, position.y) + imagePosition
        local index = indices[position]


        if topIndex >= index then
            ipsum.setColor(colors[position])
            ipsum.setBackgroundColor(backgrounds[position].color)
            ipsum.setBackgroundIntensity(backgrounds[position].intensity)
            ipsum.putTile(shape, finalTarget.x, finalTarget.y)
        end
    end

    for position, shape in pairs(shapes) do
        local finalTarget = createVector(position.x, position.y) + imagePosition

        local index = indices[position]

        if math.abs(((elapsedTime * 500 - index))) < 15 then
            ipsum.setColor("green_text")
            ipsum.setBackgroundColor("blue_brick")
            ipsum.setBackgroundIntensity(backgrounds[position].intensity)
            ipsum.putTile(shape, finalTarget.x, finalTarget.y + 1)
            topIndex = index
        end
    end
end
