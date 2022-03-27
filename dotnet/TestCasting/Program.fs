// Learn more about F# at http://fsharp.org

open System
open Python.Runtime
open FSharp.Interop.Dynamic
open FSharp.Interop.Dynamic.Operators
open PandasNet
Runtime.PythonDLL <- @"C:\Users\anibal\AppData\Local\Programs\Python\Python38\python38.dll"

PythonEngine.Initialize()

let o = Py.GIL()

let code = """
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

"""

[<EntryPoint>]
let main argv =
    let scope = Py.CreateScope()
    let _ = scope.Exec(code)
    let t1:PyObject = scope?f2()





    let mutable oo:obj = null
    let df = Codecs.Instance.TryDecode(t1, &oo)
    0 // return an integer exit code
