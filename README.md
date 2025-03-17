# Honours-Project-Repository
 My honours project and dissertation.

Sources:
Egana, B.M., 2018. Procedural level generation for a 2D platformer. https://digitalcommons.calpoly.edu/cscsp/130/
Shaker, N., Togelius, J. and Nelson, M.J., 2016. Procedural content generation in games. https://link.springer.com/content/pdf/10.1007/978-3-319-42716-4.pdf
Smith, G., 2014, April. Understanding procedural content generation: a design-centric analysis of the role of PCG in games. In Proceedings of the SIGCHI Conference on Human Factors in Computing Systems (pp. 917-926). https://dl.acm.org/doi/abs/10.1145/2556288.2557341
Alpay, B., 2024. A comparison of procedural-generated and human-designed two-dimensional platformer game levels (Master's thesis, İzmir Ekonomi Üniversitesi). https://gcris.ieu.edu.tr/handle/20.500.14365/5265
https://www.guinnessworldrecords.com/world-records/best-selling-video-game?os=0&ref=app#:~:text=The%20best-selling%20videogame%20of%20all%20time%20is%20Minecraft%2C,sales%20by%20publisher%20Microsoft%20on%2015%20October%202023. - Minecraft = best selling game of all time

Implementation Sources:
https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Physics2D.Raycast.html - used for making gaps
https://docs.unity3d.com/6000.0/Documentation/ScriptReference/RaycastHit2D.html - how to store hits from the raycast
https://github.com/chriscore/MarkovSharp - C# Markov Chains in VS
https://nickmcd.me/2019/10/30/markov-chains-for-procedural-buildings/ - Markov in Unity (Nick's blog)
https://www.sbgames.org/sbgames2018/files/papers/ComputacaoShort/188123.pdf - Markov in Unity (Thesis)
https://stackoverflow.com/questions/63106256/find-and-return-nearest-gameobject-with-tag-unity - finding objects
https://d1wqtxts1xzle7.cloudfront.net/75095043/1803-libre.pdf?1637780061=&response-content-disposition=inline%3B+filename%3DGenerative_design_in_minecraft_GDMC.pdf&Expires=1741793194&Signature=J4dxmlwE3zIoVakoKi~n-Hf8uhZl~H4Mqn9t~MDb9zDbGEe-bfs25Xr872kSCjxaDvcUezJCAPeWCHH7MZ5EOqs8aLacebalTcFnhH8~Xbm33ohKc2LMQ-NPTqC9yMfkOrcVd92Y0yDDjppXuGlGGvC6R8u~1ol9RQQgY7qjYyaRaYBVGVIeVrX21FewsUR9T6cGmYaryYXq7Or2OLdKDZPBH0FWWJzlXGfGSv2Z2G5vzlcIUhtOA-f1g-BR50X9Ub6Iq6vd9p9g0qssDAzYO~9QuhCxrY9~3LbGuZurppUfLzBDatNL7bhbRug5A~FeuyJzLgv8lIvap0M6kBOnSw__&Key-Pair-Id=APKAJLOHF5GGSLRBV4ZA - Minecraft Perlin Noise
https://citeseerx.ist.psu.edu/document?repid=rep1&type=pdf&doi=4c6bb41637a6438f22eb190d014a01cd0b0a7162 - Markov Chain History
https://dawnosaur.substack.com/p/how-minecraft-generates-worlds-you - Minecraft world generation

------------------------------------------------DEVELOPMENT-----------------------------------------------------------------------------
- Need to work out why so many gaps are only 1 long, and edit params to try to avoid 1 block of ground between gaps (make a 
    minimum number of blocks before a new gap can begin, as well as the time affecting chance?) - fixed
- Platform height needs to be adjusted to ensure player can both fit under and jump onto them - fixed
Could add in a jump mechanic for the spider if it detects ground at a higher level, may help fix spiders being stuck in small spaces?
----------------------------------------------------------------------------------------------------------------------------------------

https://thepeeps191.itch.io/spinning-coin - coin sprite