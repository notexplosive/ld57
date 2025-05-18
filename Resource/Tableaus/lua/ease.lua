local Ease = {}

function Ease.quadSlowFast(x)
    return x * x
end

function Ease.quadFastSlow(x)
    return 1 - Ease.quadSlowFast(1 - x);
end

return Ease
