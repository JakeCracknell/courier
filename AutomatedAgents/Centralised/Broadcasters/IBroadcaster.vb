Public Interface IBroadcaster
    Sub BroadcastJob(ByVal Job As CourierJob)
    Sub RegisterContractor(ByVal Contractor As IContractor)
    Sub AwardJobs()

    'For CNP5
    Function ReallocateJobs(ByVal Owner As IContractor, ByVal RetractableJobs As List(Of CourierJob)) As List(Of CourierJob)
End Interface
