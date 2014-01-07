X-Image Specifications
=====

Control your images' size, crop, quality and more from the querystring.  Perfect fit as your CDN origin.

Examples
---
- http://i.example.com/house.png?w=100
- http://i.example.com/house@2x.gif?w=100&h=200&v=crop
- http://i.example.com/house@2x.jpg?w=100&h=200&v=black&q=35kb&c=1h


Querystring
---
- w=100 (alt: width)
- h=200 (alt: height)
- v=clip,crop,stretch,aabbcc (alt: void)
- q=50 (append kb as alternative to % default) (alt: quality)
- f=jpg,png,gif (alt: name.jpg or f=jpg or format=jpg)
- r=2 (alt: @2x or resolution=2)
- c=1s,2m,3d (must include units, alt: cache)


Headers
---
Meta data about the image can be included in the response headers.  Request using h=x-image-original-width,x-image-original-height (alt: headers).  The server can optionally choose to send these down even if not requested.  (Note: This option works great with HEAD in addition to GET.)
- X-Image-Original-Width
- X-Image-Original-Height
- X-Image-Average-Color
- X-Image-Dominant-Color
- X-Image-Palette
- X-Image-Histogram


(config piece sets the max image size, for hack-protection)

