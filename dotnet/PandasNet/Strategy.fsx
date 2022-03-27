
(*
長短期策略可能會因為在不同尺度觀察得到結論而產生
隨機產生策略：

1. 選定如何觀察
2. 觀察
3. 找尋特徵
4. 計算勝率賺賠比
5. 訂定採用標準(閾值)
6. 符合標準，return，不符合，loop again
 
系統流程：
1. 產生5000種策略
2. 給1000萬交易者隨機搭配策略與參數
3. 模擬撮合 


*)

#r @"C:\git\pythonnet\pythonnet\runtime\Python.Runtime.dll"
#r @"C:\git\pandasnet\dotnet\PandasNet\bin\Debug\netstandard2.0\PandasNet.dll"
#r @"nuget:FSharp.Interop.Dynamic"
#r @"nuget:Deedle"
open Python.Runtime
open FSharp.Interop.Dynamic
open FSharp.Interop.Dynamic.Operators
Runtime.PythonDLL <- @"C:\Users\anibal\AppData\Local\Programs\Python\Python38\python38.dll"

PythonEngine.Initialize()


let o = Py.GIL()

let np = Py.Import("numpy")

let cos:obj = np?cos(np?pi ?*? 2)
cos.GetType().Name

let b: obj = np?array([| 6.; 5.; 4. |], Dyn.namedArg "dtype" np?int32)
printfn "%O" b

b.GetType().Name


//let p = PythonEngine.Compile("",@"C:\coldfar_py\experiments\007_pythonnet\main001.py",RunFlagType.File)


//printfn "%O" (p?f())


let scope = Py.CreateScope()
scope.Exec("""
import pandas as pd
import pickle

def f():
    return pd.DataFrame({"a":[1,2,3], "b":[4,5,6]})

def tf_to_int(v):
    if v:
        return 1.0
    else:
        return 0.0

def f2():
    with open('C:\coldfar_py\data_pickled\es_data_001_smallbox.df_p_c.pickle', 'rb') as f:
        df = pickle.load(f).astype({'if_highly_overlapped': 'bool'})
        df.reset_index(inplace=True)
        #df["volume"] = df["volume"].apply(lambda v: float(v))
        #df["DIF_mean_Δ"] = df["DIF_mean_Δ"].apply(lambda v: float(v))
        #df["OSC_mean_Δ"] = df["OSC_mean_Δ"].apply(lambda v: float(v))
        #df["g"] = df["g"].apply(lambda v: float(v))
        #df["if_highly_overlapped"] = df["if_highly_overlapped"].apply(lambda v: tf_to_int(v))
        #df["if_highly_overlapped"] = df["if_highly_overlapped"].apply(lambda v: str(v))

        
        return df
    
""")
let t1:PyObject = scope?f2()

printfn "%O" t1

open PandasNet

let mutable oo:obj = null
Codecs.Instance.TryDecode(t1, &oo)

oo.GetType().FullName

let oodict = (oo :?> System.Collections.Generic.Dictionary<string, System.Array>)



#r @"nuget:Deedle"
#r @"nuget:FSharp.Charting"
#r @"nuget:System.Windows.Forms.DataVisualization, 1.0.0-prerelease.20110.1"
#I __SOURCE_DIRECTORY__
//#I "lib/net45"
//#r @"nuget:System.Windows.Forms"
#r @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Windows.Forms.dll"
#nowarn "211"
open System.Linq
open FSharp.Charting
module FsiAutoShow = 
    fsi.AddPrinter(fun (ch:FSharp.Charting.ChartTypes.GenericChart) -> ch.ShowChart() |> ignore; "(Chart)")

do fsi.AddPrinter(fun (printer:Deedle.Internal.IFsiFormattable) -> "\n" + (printer.Format()))
open Deedle

let coldfar_index = oodict.["datekey"]:?> System.DateTime[]
oodict.Keys |> Seq.toArray
//(oodict.["34ma"] :> System.Collections.IEnumerable).Cast<obj>() |> Seq.item 0

let vv = 
    oodict 
    |> Seq.map (fun kvp -> 
        let arr = kvp.Value
        kvp.Key, (arr :> System.Collections.IEnumerable).Cast<obj>())
    |> Seq.map (fun (k, seq_) -> k, (seq_ |> Seq.item 1000).GetType().Name)
    |> Seq.toArray

let cast (index:'T[]) k (idx:int) (a:System.Array) =
    let seqArr = a :> System.Collections.IEnumerable
    let itmx = seqArr.Cast<obj>() |> Seq.item idx 
    printfn "k %s: %s" k (itmx.GetType().FullName)
    match itmx with
    | :? string ->
        printfn "string column: %s" k
        Frame.ofColumns [k => Series<'T, string>(index, seqArr.Cast<string>())]
    | :? int ->
        printfn "int column: %s" k
        Frame.ofColumns [k => Series<'T, int>(index, seqArr.Cast<int>())]
    | :? float ->
        printfn "float column: %s" k
        Frame.ofColumns [k => Series<'T, float>(index, seqArr.Cast<float>())]
    | :? int64 ->
        printfn "int64 column: %s" k
        Frame.ofColumns [k => Series<'T, int64>(index, seqArr.Cast<int64>())]

let coldfar_frame_seq =
    oodict
    |> Seq.choose (fun kvp -> 
        let k = kvp.Key
        if k = "datekey" then
            None
        else
            cast coldfar_index k 5000 kvp.Value |> Some
        )
    |> Seq.cache
let coldfar_frame_merged =
    coldfar_frame_seq
    |> fun fs ->
        (fs |> Seq.item 0).Merge (fs |> Seq.skip 1)

coldfar_frame_merged.Columns.ValueCount

let odm = coldfar_frame_merged.Columns.["OSC_mean_comp"]

odm.Vector.GetType().FullName

odm.Vector


odm.Values.GetType().FullName

let dm : Series<System.DateTime, int> = coldfar_frame_merged.Columns.["DIF_mean_Δ"].As()
dm.GetAt 0

let cc = ((coldfar_frame_seq |> Seq.last).Columns.GetAt 0)
cc.GetType().FullName
let ss : Series<System.DateTime, string> = cc.As()
((coldfar_frame_seq |> Seq.last).Columns.GetAt 0).Values
(coldfar_frame_seq |> Seq.last).GetType().FullName



let c0 = coldfar_frame_merged.Columns.GetAt 0
let c0a : Series<System.DateTime, float>  = c0.As()

c0a.GetAt 0 

coldfar_frame_merged.["DIF_mean_Δ"].GetAt 0


let coldfar_dataset =
    oodict
    |> Seq.choose (fun kvp -> 
        let k = kvp.Key
        if k = "datekey" then
            None
        else
            let v = (kvp.Value :> System.Collections.IEnumerable).Cast<obj>()
            let s = Series(coldfar_index, v)
            Some (k => s)
        )


let df2 = Frame.ofColumns coldfar_dataset

let odm2 = df2.Columns.["OSC_mean_comp"]

odm2.Vector
odm2.Vector.GetType().FullName

(coldfar_frame_seq.Rows.GetAt 0).Get "open"

let cOpen = df2.["DIF_mean_Δ"]

cOpen.GetAt 0

let df4 = 
  [ ("Monday", "Tomas", 1.0); ("Tuesday", "Adam", 2.1)
    ("Tuesday", "Tomas", 4.0); ("Wednesday", "Tomas", -5.4) ]
  |> Frame.ofValues


let coldfar_series = oodict.ToSeries()


coldfar_series.cou

coldfar_series |> Frame.ofColumns



let masterDict =
    dict [
        "Corn Future" , dict ["2009-09-01", 316.69; "2009-09-02", 316.09; "2009-09-03", 316.33];
        "Wheat Future", dict ["2009-09-01", 214.4 ; "2009-09-02", 223.86; "2009-09-03", 234.11];
        ]

let frame = 
    masterDict
    |> Seq.map(fun kv -> kv.Key, kv.Value 
                                 |> Seq.map(fun nkv -> nkv.Key, nkv.Value)
                                 |> Series.ofObservations)
    |> Frame.ofColumns