local Vector = {}

Vector["new"] = function(x, y)
    local v = {
        x = x,
        y = y
    }
    setmetatable(v, Vector)
    return v
end

Vector["__index"] = Vector
Vector["type"] = "Vector"

Vector["__add"] = function(a, b)
    return Vector.new(a.x + b.x, a.y + b.y)
end

Vector["__sub"] = function(a, b)
    return Vector.new(a.x - b.x, a.y - b.y)
end

Vector["__mul"] = function(a, b)
    local scaler = 0
    local v = Vector.new(0, 0)

    if type(a) == "number" then
        scaler = a
        v = b
    elseif type(b) == "number" then
        scaler = b
        v = a
    end

    return Vector.new(v.x * scaler, v.y * scaler)
end

Vector["__div"] = function(a, b)
    local scaler = 0
    local v = Vector.new(0, 0)

    if type(a) == "number" then
        scaler = a
        v = b
    elseif type(b) == "number" then
        scaler = b
        v = a
    end

    return Vector.new(v.x / scaler, v.y / scaler)
end

function Vector.lerp(source, destination, percent)
    return source + (destination - source) * percent
end

return Vector
