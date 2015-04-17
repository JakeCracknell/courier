Imports System.Text

Namespace LogMessages
    Module LogMessages
        Function PickFail(ByVal JobID As Integer) As String
            Return String.Format("Pickup failed for job {0}. It has been cancelled.", JobID)
        End Function
        Function DeliveryFail(ByVal JobID As Integer, ByVal DepotKMAway As Double) As String
            Return String.Format("Delivery failed for job {0}. It will be delivered to the nearest depot, {1} km away.", _
                                 JobID, Math.Round(DepotKMAway, 1))
        End Function
        Function PickSuccess(ByVal JobID As Integer) As String
            Return String.Format("Job {0} was successfully picked up.", JobID)
        End Function
        Function DeliverySuccess(ByVal JobID As Integer, ByVal TimeLeft As TimeSpan) As String
            Return String.Format("Job {0} was successfully delivered, {1} minutes early", JobID, CInt(TimeLeft.TotalMinutes))
        End Function
        Function DeliveryLate(ByVal JobID As Integer, ByVal TimeLate As TimeSpan, ByVal Refund As Decimal) As String
            Return String.Format("Job {0} was delivered {1} minutes late. {2} has been refunded.", _
                                 JobID, CInt(TimeLate.TotalMinutes), FormatCurrency(Refund))
        End Function
        Function Refuel(ByVal Litres As Double, ByVal Cost As Decimal) As String
            Return String.Format("Agent has refuelled: {0} L at a cost of {1}.", Math.Round(Litres, 2), FormatCurrency(Cost))
        End Function
        Function EmergencyRefuel(ByVal LitresLeft As Double, ByVal DistanceToFuel As Double) As String
            Return String.Format("Deferring jobs to refuel at a station {1} km away. Reserves are low ({0} L)", _
                                 Math.Round(LitresLeft, 2), Math.Round(DistanceToFuel, 1))
        End Function


        Function JobBroadcasted(ByVal JobID As Integer) As String
            Return String.Format("Job {0} has been broadcasted to all contractors for bidding", JobID)
        End Function
        Function BidsReceived(ByVal JobID As Integer, ByVal Bids As List(Of Double)) As String
            Dim SB As New StringBuilder(String.Format("Job {0} has received {1} ", JobID, Bids.Count))
            SB.Append(If(Bids.Count = 1, "bid:", "bids:"))
            For Each Bid As Double In Bids
                SB.Append("    ")
                SB.Append(Math.Round(Bid, 2))
            Next
            SB.Append(".")
            Return SB.ToString
        End Function
        Function JobRefused(ByVal JobID As Integer) As String
            Return String.Format("Job {0} has been refused, as no agent placed a bid.", JobID)
        End Function
        Function JobAwarded(ByVal JobID As Integer, ByVal Bid As Double) As String
            Return String.Format("Job {0} has been awarded to the contractor bidding {1}.", JobID, Math.Round(Bid, 2))
        End Function

        Function CNP5JobsSentForTransfer(ByVal JobCount As Integer) As String
            Return String.Format("{0} {1} been sent back to the server for reauction", JobCount, _
                                 If(JobCount = 1, "job has", "jobs have"))
        End Function
        Function CNP5JobTransfer(ByVal JobID As Integer, ByVal AgentIDFrom As Integer, ByVal AgentIDTo As Integer) As String
            Return String.Format("Job {0} has been reallocated from agent {1} to agent {2}", _
                                 JobID, AgentIDFrom, AgentIDTo)
        End Function
        Function CNP5JobTransferResult(ByVal TransferredCount As Integer, ByVal UntransferredCount As Integer) As String
            Return String.Format("Auction results: {0} successfully reallocated, {1} could not be reallocated.", _
                                 TransferredCount, UntransferredCount)
        End Function

    End Module
End Namespace