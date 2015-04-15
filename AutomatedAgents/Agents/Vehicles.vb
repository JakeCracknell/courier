Namespace Vehicles
    Public Module Vehicles
        Public Const MILE_LENGTH_IN_KM = 1.609344
        Public Const LITRES_TO_A_GALLON = 3.785

        Public Enum Type
            CAR
            VAN
            TRUCK '7.5 tonnes
        End Enum

        Function Name(ByVal Type As Type) As String
            Select Case Type
                Case Vehicles.Type.CAR
                    Return "Car"
                Case Vehicles.Type.VAN
                    Return "Van"
                Case Vehicles.Type.TRUCK
                    Return "Truck"
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
                Case Vehicles.Type.TRUCK
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
                Case Else
                    Return 0.0
            End Select
        End Function

        'Function approximated from data here: http://blog.automatic.com/cost-speeding-save-little-time-spend-lot-money/
        'Multipliers
        Function FuelEconomy(ByVal Type As Type, ByVal DistanceTravelledKM As Double) As Double
            If Not DistanceTravelledKM > 0 Then
                Return 0
            End If

            Dim Multiplier As Double

            Select Case Type
                Case Vehicles.Type.CAR
                    Multiplier = 1
                Case Vehicles.Type.VAN
                    Multiplier = 0.92
                Case Vehicles.Type.TRUCK
                    Multiplier = 0.55
            End Select

            Dim MilesTravelled As Double = DistanceTravelledKM / MILE_LENGTH_IN_KM
            Dim SpeedMPH As Double = MilesTravelled * 3600
            Dim MPG As Double = Multiplier * (-0.0119 * SpeedMPH ^ 2 + SpeedMPH * 1.2754)
            Dim FuelUsedInGallons As Double = MilesTravelled / MPG
            Return FuelUsedInGallons * LITRES_TO_A_GALLON
        End Function

        'UK Petrol Prices for Thursday 9th April 2015
        'http://www.petrolprices.com/
        Function FuelCost(ByVal Type As Type, ByVal Litres As Double) As Double
            Debug.Assert(Litres >= 0)
            Select Case Type
                Case Vehicles.Type.CAR
                    'Unleaded regular
                    Return Math.Round(Litres * 1.1327, 2)
                Case Vehicles.Type.VAN, Vehicles.Type.TRUCK
                    'Diesel
                    Return Math.Round(Litres * 1.1882, 2)
                Case Else
                    Return 0.0
            End Select
        End Function


    End Module
End Namespace
