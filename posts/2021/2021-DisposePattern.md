# Its time to reconsider the usefulness of the classical Dispose Pattern

# The classical Dispose Pattern considered useless

I think the time is come to face it: the Dispose pattern in the form the most people used it is useless. But to understand why and if this is the case we need to undertand what problem it should solve.

The managed envrionment like the CLR is very good in reclaiming not used memory, but not that good when it comes to opaque resources that the CLR has no clue about.

For instance, if the user calls some WinAPI, 