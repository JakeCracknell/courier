Namespace Vehicles
    Public Module Vehicles
        Public Const MILE_LENGTH_IN_KM As Double = 1.609344
        Public Const LITRES_TO_A_GALLON As Double = 3.785
        Public Const OPTIMAL_KMH As Double = 86.242

        Public Enum Type
            CAR
            VAN
            TRUCK
            BAD_TRUCK
        End Enum

        Function Name(ByVal Type As Type) As String
            Select Case Type
                Case Vehicles.Type.CAR
                    Return "Car"
                Case Vehicles.Type.VAN
                    Return "Van"
                Case Vehicles.Type.TRUCK
                    Return "Truck"
                Case Vehicles.Type.BAD_TRUCK
                    Return "Bad Truck"
                Case Else
                    Return ""
            End Select
        End Function

        'A 2 seat sofa is 1.33m^3
        Function Capacity(ByVal Type As Type) As Double
            Select Case Type
                Case Vehicles.Type.CAR
                    Return 1.0
                Case Vehicles.Type.VAN
                    Return 2.0
                Case Vehicles.Type.TRUCK, Vehicles.Type.BAD_TRUCK
                    Return 8.0
                Case Else
                    Return 0.0
            End Select
        End Function

        Function FuelTankSize(ByVal Type As Type) As Double
            Select Case Type
                Case Vehicles.Type.CAR
                    Return 50.0
                Case Vehicles.Type.VAN
                    Return 80.0
                Case Vehicles.Type.TRUCK
                    Return 92.0
                Case Vehicles.Type.BAD_TRUCK
                    Return 10.0
                Case Else
                    Return 0.0
            End Select
        End Function

        'Function approximated from data here: http://blog.automatic.com/cost-speeding-save-little-time-spend-lot-money/
        'Multipliers
        Function FuelEconomy(ByVal Type As Type, ByVal KMTravelledInSecond As Double) As Double
            If Not KMTravelledInSecond > 0 Then
                Return 0
            End If

            Dim Multiplier As Double

            Select Case Type
                Case Vehicles.Type.CAR
                    Multiplier = 1
                Case Vehicles.Type.VAN
                    Multiplier = 0.92
                Case Vehicles.Type.TRUCK, Vehicles.Type.BAD_TRUCK
                    Multiplier = 0.55
            End Select

            Dim MilesTravelled As Double = KMTravelledInSecond / MILE_LENGTH_IN_KM
            Dim SpeedMPH As Double = MilesTravelled * 3600
            Dim MPG As Double = Multiplier * (-0.0119 * SpeedMPH ^ 2 + SpeedMPH * 1.2754)
            Dim FuelUsedInGallons As Double = MilesTravelled / MPG
            Return FuelUsedInGallons * LITRES_TO_A_GALLON
        End Function

        Function OptimalFuelUsageAndTime(ByVal KM As Double, ByVal MaxSpeed As Double, _
                                         Optional ByVal VehicleType As Type = Type.CAR) As Tuple(Of Double, Double)
            Dim OptimalSpeedKMS As Double = Math.Min(MaxSpeed, OPTIMAL_KMH) / 3600
            Dim FuelUsedInOneSecond As Double = FuelEconomy(VehicleType, OptimalSpeedKMS)
            Dim TimeToTravelInSeconds As Double = KM / OptimalSpeedKMS

            Return New Tuple(Of Double, Double)(TimeToTravelInSeconds * FuelUsedInOneSecond, TimeToTravelInSeconds)
        End Function

        'UK Petrol Prices for Thursday 9th April 2015
        'http://www.petrolprices.com/
        Function FuelCost(ByVal Type As Type, ByVal Litres As Double) As Double
            Select Case Type
                Case Vehicles.Type.CAR
                    'Unleaded regular
                    Return Math.Round(Litres * 1.1327, 2)
                Case Vehicles.Type.VAN, Vehicles.Type.TRUCK, Vehicles.Type.BAD_TRUCK
                    'Diesel
                    Return Math.Round(Litres * 1.1882, 2)
                Case Else
                    Return 0.0
            End Select
        End Function


    End Module
End Namespace
