Namespace Vehicles
    Public Module Vehicles
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

        Function FuelEconomy(ByVal Type As Type, ByVal DistanceTravelledKM As Double) As Double
            'For now just say fixed KPL. Note 100 mpg = 35.4 kpl
            Debug.Assert(DistanceTravelledKM >= 0)
            Select Case Type
                Case Vehicles.Type.CAR
                    Return DistanceTravelledKM / 17.7
                Case Vehicles.Type.VAN
                    Return DistanceTravelledKM / 11
                Case Vehicles.Type.TRUCK
                    Return DistanceTravelledKM / 8
                Case Else
                    Return 0.0
            End Select
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
