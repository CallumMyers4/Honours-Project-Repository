# Honours-Project-Repository
 My honours project and dissertation.

Sources:
Egana, B.M., 2018. Procedural level generation for a 2D platformer. https://digitalcommons.calpoly.edu/cscsp/130/
Shaker, N., Togelius, J. and Nelson, M.J., 2016. Procedural content generation in games. https://link.springer.com/content/pdf/10.1007/978-3-319-42716-4.pdf
Smith, G., 2014, April. Understanding procedural content generation: a design-centric analysis of the role of PCG in games. In Proceedings of the SIGCHI Conference on Human Factors in Computing Systems (pp. 917-926). https://dl.acm.org/doi/abs/10.1145/2556288.2557341
Alpay, B., 2024. A comparison of procedural-generated and human-designed two-dimensional platformer game levels (Master's thesis, İzmir Ekonomi Üniversitesi). https://gcris.ieu.edu.tr/handle/20.500.14365/5265

Implementation Sources:
https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Physics2D.Raycast.html - used for making gaps
https://docs.unity3d.com/6000.0/Documentation/ScriptReference/RaycastHit2D.html - how to store hits from the raycast
https://github.com/chriscore/MarkovSharp - C# Markov Chains in VS
https://nickmcd.me/2019/10/30/markov-chains-for-procedural-buildings/ - Markov in Unity (Nick's blog)
https://www.sbgames.org/sbgames2018/files/papers/ComputacaoShort/188123.pdf - Markov in Unity (Thesis)
https://stackoverflow.com/questions/63106256/find-and-return-nearest-gameobject-with-tag-unity - finding objects

------------------------------------------------DEVELOPMENT-----------------------------------------------------------------------------
- Need to work out why so many gaps are only 1 long, and edit params to try to avoid 1 block of ground between gaps (make a 
    minimum number of blocks before a new gap can begin, as well as the time affecting chance?) - fixed
- Platform height needs to be adjusted to ensure player can both fit under and jump onto them - fixed
Could add in a jump mechanic for the spider if it detects ground at a higher level, may help fix spiders being stuck in small spaces?
----------------------------------------------------------------------------------------------------------------------------------------

https://thepeeps191.itch.io/spinning-coin - coin sprite