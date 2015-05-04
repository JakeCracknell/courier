Public Enum ContractNetPolicy
    'Agents only fulfil one job at a time.
    CNP1 'Do not bid unless idle
    CNP2 'New jobs are appended to the end of the plan
    CNP3 'New jobs can cause the plan to be reordered, but no multi-carriage

    'Agents fulfil many jobs at a time
    CNP4 'Agents reorder waypoints in an optimal manner, minimising lateness and then total time.
    CNP5 'Agents tentatively bid for jobs, can ask broadcaster to reallocate when circumstances change.
End Enum