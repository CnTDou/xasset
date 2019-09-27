using Plugins.XAsset;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Plugins.XAsset
{
    public partial class Asset : IResInfo
    { 
        private ResInfoState state;
 
        public Object Obj
        {
            get
            {
                return asset; 
            }
        }

        public string Name
        {
            get { return asset != null ? asset.name : string.Empty; }
        }

        public int RefCount
        {
            get { return refCount; }
        }

        public bool IsDone
        {
            get { return isDone; }
        }
 
        public ResInfoState State
        {
            get
            {
                if (state != ResInfoState.Error)
                    state = (ResInfoState) (int) loadState;
                return state;
            }
        }

        public string Error {get { return error; } } 
       
    }
}