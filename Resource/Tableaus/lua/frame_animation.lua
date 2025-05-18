local frameAnimation = {}

function frameAnimation.drawFrame(image, frames, x, y, w, h, t)
    local frameIndex = math.floor(1 + (t % (#frames + 1)))
    if frameIndex > #frames then
        frameIndex = 1
    end

    ipsum:putImageSlice(image, x, y, frames[frameIndex].x, frames[frameIndex].y, w, h)
end

return frameAnimation
