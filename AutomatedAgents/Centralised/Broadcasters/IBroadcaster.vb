Public Interface IBroadcaster
    'The exact implementation of IBroadcaster can vary, but the method to which
    'it allocates jobs to agents should not be the result of centralised
    'processing. For example, it should not discriminate on the properties of
    'the job, such as deadline or location

    Sub BroadcastJob(ByVal Job As CourierJob)
    Sub RegisterContractor(ByVal Contractor As IContractor)
    Sub AwardJobs()

    'For CNP5
    Function ReallocateJobs(ByVal Owner As IContractor, ByVal RetractableJobs As List(Of CourierJob)) As List(Of CourierJob)
End Interface
