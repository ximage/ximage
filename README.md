X-Image Specifications
=====

Control your images' size, crop, quality and more from the querystring.  Perfect fit as your CDN origin.

Querystring
---
- w (alt: width)
- h (alt: height)
- v = clip,crop,stretch,#aabbcc (alt: void)
- q = 0-100,(append kB as alternative to % default) (alt: quality)
- .jpg,.png,.gif (alt: f=jpg,png,gif or format=jpg,png,gif)
- @2x (alt: r=2 or resolution=2)
- c = (ex: c=10s, c=1d, c=1w, default is seconds) (alt: cache)

Headers
---
Meta data about the image are included in the response headers.  Request using h=x-image-width,x-image-height (alt: headers).  The server can optionally choose to send these down even if not requested.  (Note: This option works great with HEAD in addition to GET.)
- X-Image-Width
- X-Image-Height
- X-Image-Average-Color
- X-Image-Dominant-Color
- X-Image-Palette
- X-Image-Histogram

examples:
- http://i.example.com/house.png?w=100
- http://i.example.com/house@2x.gif?w=100&h=200&v=crop
- http://i.example.com/house@2x.jpg?w=100&h=200&v=black&q=35kb&c=3600

(config piece sets the max image size, for hack-protection)

