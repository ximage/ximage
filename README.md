picme
=====

Control your images' crop, size, quality and more from the querystring.  Perfect fit as your CDN origin.

notes for me:
http://www.imgix.com/docs/urlapi

docs:
- w
- h
- fit=clip,crop,scale,fill,max... (imgix)   ---or---   (mine)
- &allow=stretch,crop,upscale (stretch & crop only valid if both w & h are used)
- fm=jpg,png,gif
- q=0-100,xKB
- cache=(seconds)
 
advanced:
- crop=top,bottom,left,right,faces?
- dpr=.75,1,2
- rot
- flip
- rect
- blur
- col/int
- https?
- config piece sets the max image size (for hack-protection)
 



simpler:
- w
- h
- allow = upscale,stretch,crop (stretch & crop only valid if both w & h are used)
 
advanced simpler:
- dpr = .75,1,2 (what about @2X?)
- fm = jpg,png,gif
- q = 0-100,xKB
- cache = (seconds)

meta
- dimensions
- average color
- dominant color


Oooo, here's a good staring FAQ: http://www.imgix.com/faq
Do I need to upload my images to imgix?
imgix acts as a proxy to your existing images, so no uploads are necessary. We can pull your images from your own web folder, web proxy, or Amazon S3 on demand, just give us the details and we can do the heavy lifting.
