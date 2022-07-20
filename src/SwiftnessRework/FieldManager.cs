using System.Collections.Generic;
using System;

namespace SwiftnessRework
{

    abstract public class FieldManager<Tkey, TfieldVal>
    {

        public class RefHolder
        {
            public WeakReference weakRef;
            public TfieldVal value;
        }

        public Dictionary<Tkey, List<RefHolder>> fieldDict;
        public int defaultListCap = 10;

        // some custom method to reduce "key collision"
        // some value which differentiates the instances but doesn't change through lifetime of the instance
        // example could be inst.GetHashCode().toString() + instance.GetType().Name if associated types are extended often
        // default inst.GetHashCode() could still be good enough in terms of functionality
        public abstract Tkey ComputeKey(object inst);

        public FieldManager()
        {
            fieldDict = new Dictionary<Tkey, List<RefHolder>>();
        }

        public FieldManager(int cap, int defaultListCap)
        {
            fieldDict = new Dictionary<Tkey, List<RefHolder>>(cap);
            this.defaultListCap = defaultListCap;
        }


        public void AddField(object inst, TfieldVal val)
        {
            var dict = fieldDict;
            var key = ComputeKey(inst);
            if (!dict.ContainsKey(key))
            {
                dict.Add(key, new List<RefHolder>(defaultListCap) { new RefHolder() { weakRef = new WeakReference(inst, false), value = val } });
            }
            else
            {
                dict[key].Add(new RefHolder() { weakRef = new WeakReference(inst, false), value = val });
            }

        }

        bool LoopBucket(object inst, List<RefHolder> bucket, out TfieldVal result, out int index)
        {

            result = default(TfieldVal);
            index = -1;
            var i = 0;

            if (bucket.Count == 1)
            {
                result = bucket[0].value;
                index = 0;
                return true;
            }

            // if key distribution is good this loop will rarely be executed
            foreach (var refholder in bucket.ToArray())
            {
                // might as well remove dead values while looping
                if (!refholder.weakRef.IsAlive)
                {
                    bucket.Remove(refholder);
                    continue;
                }
                // can target become null right before equality is tested?
                var tempStrongRef = refholder.weakRef.Target;
                if (inst.Equals(tempStrongRef))
                {

                    result = refholder.value;
                    index = i;
                    return true;
                }
                i++;

            }
            return false;
            
        }

        public TfieldVal GetVal(object inst)
        {
            var key = ComputeKey(inst);
            if (fieldDict.TryGetValue(key, out List<RefHolder> bucket))
            {
                if (LoopBucket(inst, bucket, out TfieldVal outValue, out int i))
                {
                    return outValue;
                }
                else
                {
                    throw new Exception($"GetVal: no ENTRY with {key} key, {i} index (object: {inst})");
                }
            }
            else
            {
                throw new Exception($"GetVal: no BUCKET with {ComputeKey(inst)} key (object: {inst})");
            }
            

        }

        public void SetVal(object inst, TfieldVal val)
        {
            var key = ComputeKey(inst);
            if (fieldDict.TryGetValue(key, out List<RefHolder> bucket))
            {
                if (LoopBucket(inst, bucket, out TfieldVal outValue, out int i))
                {
                    fieldDict[key][i].value = val;
                }
                else
                {
                    throw new Exception($"Setval: no ENTRY with {key} key (object: {inst})");
                }

            }
            else
            {
                throw new Exception($"SetVal: no BUCKET with {ComputeKey(inst)} key (object: {inst})");
            }
        }



        public void CullDestroyed()
        {
            var keys2Remove = new List<Tkey>();
            foreach (var kv in fieldDict)
            {
                foreach (var bucketEntry in kv.Value.ToArray())
                {
                    if (!bucketEntry.weakRef.IsAlive)
                    {
                        fieldDict[kv.Key].Remove(bucketEntry);
                    }
                }
                if (fieldDict[kv.Key].Count == 0)
                {
                    keys2Remove.Add(kv.Key);
                }

            }
            foreach (var k in keys2Remove)
            {
                fieldDict.Remove(k);
            }


        }



    }
}