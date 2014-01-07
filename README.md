X-Image Specifications
=====

Control your images' size, crop, quality and more from the querystring.  Great to use as your CDN origin.

Examples
---
- http://i.example.com/house.png?w=100
- http://i.example.com/house@2x.gif?w=100&h=200&v=crop
- http://i.example.com/house@2x.jpg?w=100&h=200&v=ffffff&q=35kb&c=1h


Querystring
---
- **Width** `w=100` 
- **Height** `h=200`  It is not required to use `w` and `h` together.  If one is missing, the other's size will be implicitly determined.  By default, to avoid wasteful bandwidth, images will not be scaled up beyond their original dimensions.  If up-sampling is desired, append an `!` to the end of the number to force the size, e.g. `?w=1000!&h=2000!`.
- **Void** `v=clip,crop,stretch,ff3366`  This parameter is only valid when both `w` and `h` are specified and the target aspect ratio doesn't match the original aspect ratio thus creating void space on the top/bottom or left/right.  
  - `clip` (default) resizes the image proportionally until it fits within the `w`x`h` bounding box.  The void spaces on the top/bottom or left/right are not included in the resulting image.  This does mean that, if the `w`/`h` parameters are not proportional to the original image's width/heigth then, due to clipping the void space, either the resulting width will be smaller than `w` or the resulting height will be smaller than `h`.
  - `crop` will resize the image proportionally until the image completely fills the `w`x`h` bounding box.  It is possible to lose the top/bottom or the left/right edges of the resulting image if the aspect ratios of the image and bounding box don't match.
  - `stretch` will force the image into the specified dimensions even if it must stretch horizontally or vertically.
  - `<color>` (e.g. ff3366) will fill the void space created on the top/bottom or left/right with any color.  Don't use a `#`, that's reserved according to the HTTP spec.
- **Quality** `q=50`  While % is the default, you can target a maximum response size by appending 'b', 'kb' or 'mb' (bytes, not bits).  Be aware that the server may need to perform multiple resizings as trial and error before yielding the result, i.e. it can be an expensive operation.  Also know that some response sizes are impossible thus resulting in a `400 Bad Request`.
- **Format** `f=jpg,png,gif` (alt: change the file extension `imagename.jpg`)  If both the file extension and querystring parameter `f` are specified, it will defer to the querystring parameter.  If nothing is specified, it defaults to the same format as the original image.
- **Resolution** `r=2` (alt: `@2x`)  The default is 1 and the only other value is 2.  Useful for retina displays.
- **Cache** `c=1s,2m,3d`  The seconds, minutes or days units are required.

In each case, the full word can be used in place of the single-letter parameter, e.g. `?width=100&height=200`



Headers
---
Meta data about the image can be included in the response headers.  Request these using a comma-separated list, e.g. `h=x-image-original-width,x-image-original-height` (alt: `headers`).  The server can optionally choose to send these down even if not requested.  (Note: This option works great with `HEAD` in addition to `GET`.)
- `X-Image-Original-Width`
- `X-Image-Original-Height`
- `X-Image-Average-Color`
- `X-Image-Dominant-Color`
- `X-Image-Palette`
- `X-Image-Histogram`
