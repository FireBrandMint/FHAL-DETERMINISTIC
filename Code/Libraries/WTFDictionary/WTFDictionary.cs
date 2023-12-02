using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


//Class that isn't a failure (surprisingly)
//Can be used for col detection easely, gg.

///<summary>
///Sorted dictionary with sqrt search and minimum
///memory allocation.
///Works with fast binary search method that loops
///sqrt(DictionaryAmountOfValue) times.
///Developed with hate by yours truly,
///Guliver Jham.
///</summary>
public class WTFDictionary<K, T>
{
    //int LastGotHash = 0;
    //int LastGot = 0;

    List<KeyValuePair<int, T>> MasterList;

    int Lenght = 0;

    public T this[K key]
    {
        get
        {
            int trueKey = key.GetHashCode();

            /*if(trueKey > LastGotHash)
            {
                int subjectNum = LastGot + 1;
                if(subjectNum < Length)
                {
                    var subject = MasterList[subjectNum];
                    if(subject.Key == trueKey)
                    {
                        LastGot = subjectNum;
                        LastGotHash = trueKey;
                        return subject.Value;
                    }
                }
            }*/

            var search = Find(trueKey);

            var valu = MasterList[search.ind0];

            #if DEBUG
            if(valu.Key != trueKey)
            {
                foreach(var kv in MasterList)
                {
                    if(kv.Key == trueKey)
                    {
                        throw new IndexOutOfRangeException($"Index {trueKey} exists but the dictionary can't find it!");
                    }
                }

                throw new IndexOutOfRangeException($"Index {trueKey} doesn't exist!");
            }
            #endif

            //LastGotHash = trueKey;
            //LastGot = search[0];

            return valu.Value;
        }
        set
        {
            int trueKey = key.GetHashCode();

            var search = Find(trueKey);

            #if DEBUG
            var valu = MasterList[search.ind0];
            if(valu.Key != trueKey) throw new IndexOutOfRangeException();
            #endif

            MasterList[search.ind0] = new KeyValuePair<int, T>(trueKey, value);
        }
    }

    public WTFDictionary()
    {
        MasterList = new List<KeyValuePair<int, T>>();
    }

    public WTFDictionary(int capacity)
    {
        MasterList = new List<KeyValuePair<int, T>>(capacity);
    }

    public void Add(K key, T value)
    {
        int keyTrue = key.GetHashCode();

        var addValu = new KeyValuePair<int, T>(keyTrue, value);

        if(MasterList.Count == 0 || MasterList[Lenght - 1].Key < keyTrue)
        {
            MasterList.Add(addValu);

            ++Lenght;

            return;
        }

        var searchResult = Find(keyTrue);

        int indexFirst = searchResult.ind0;

        int indexLast = searchResult.ind1;

        bool oneIndexScenario = indexFirst == indexLast;

        if (oneIndexScenario)
        {
            int indKey = MasterList[indexFirst].Key;

            //If is less.
            if(indKey > keyTrue)
            {
                //insert on the left
                MasterList.Insert(indexFirst, addValu);
            }
            else
            {
                int keyNode = MasterList[indexFirst].Key;
                //If is same.
                if(keyNode == keyTrue)
                {
                    throw new Exception("Same hash code.");
                }

                //Not same therefore is higher.

                //if is last.
                if(indexFirst == MasterList.Count - 1)
                {
                    MasterList.Add(addValu);
                }
                else MasterList.Insert(indexFirst + 1, addValu);
            }
        }
        else
        {
            //Left is lower or equal.

            int keyNode = MasterList[indexFirst].Key;
            
            //If its equal, EXCEPTION.
            if(keyNode == keyTrue)
            {
                throw new Exception("Same hash code.");
            }

            //It is lower then, insert it on the front.
            MasterList.Insert(indexLast, addValu);
        }

        ++Lenght;
    }
    /// <summary>
    /// returns true if hash existed.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool AddIfNonexist(K key, T value)
    {
        int keyTrue = key.GetHashCode();

        var addValu = new KeyValuePair<int, T>(keyTrue, value);

        if(MasterList.Count == 0 || MasterList[Lenght - 1].Key < keyTrue)
        {
            MasterList.Add(addValu);

            ++Lenght;

            return false;
        }

        var searchResult = Find(keyTrue);

        int indexFirst = searchResult.ind0;

        int indexLast = searchResult.ind1;

        int indKey = MasterList[indexFirst].Key;

        if(indKey == keyTrue) return true;

        bool oneIndexScenario = indexFirst == indexLast;

        if (oneIndexScenario)
        {

            //If is less.
            if(indKey > keyTrue)
            {
                //insert on the left
                MasterList.Insert(indexFirst, addValu);
            }
            else
            {
                //Not same therefore is higher.

                //if is last.
                if(indexFirst == MasterList.Count - 1)
                {
                    MasterList.Add(addValu);
                }
                else MasterList.Insert(indexFirst + 1, addValu);
            }
        }
        else
        {
            //Left is lower or equal.

            //It is lower then, insert it on the front.
            MasterList.Insert(indexLast, addValu);
        }

        ++Lenght;

        return false;
    }

    public T AddIfNonexist(K key, T value, out bool existed)
    {
        int keyTrue = key.GetHashCode();

        var addValu = new KeyValuePair<int, T>(keyTrue, value);

        if(MasterList.Count == 0 || MasterList[Lenght - 1].Key < keyTrue)
        {
            MasterList.Add(addValu);

            existed = false;

            ++Lenght;

            return value;
        }

        var searchResult = Find(keyTrue);

        int indexFirst = searchResult.ind0;

        int indexLast = searchResult.ind1;

        int indKey = MasterList[indexFirst].Key;

        if(indKey == keyTrue)
        {
            existed = true;
            return MasterList[indexFirst].Value;
        }

        bool oneIndexScenario = indexFirst == indexLast;

        if (oneIndexScenario)
        {

            //If is less.
            if(indKey > keyTrue)
            {
                //insert on the left
                MasterList.Insert(indexFirst, addValu);
            }
            else
            {
                //Not same therefore is higher.

                //if is last.
                if(indexFirst == MasterList.Count - 1)
                {
                    MasterList.Add(addValu);
                }
                else MasterList.Insert(indexFirst + 1, addValu);
            }
        }
        else
        {
            //Left is lower or equal.

            //It is lower then, insert it on the front.
            MasterList.Insert(indexLast, addValu);
        }

        existed = false;

        ++Lenght;

        return value;
    }

    public void Remove (K key)
    {
        int keyTrue = key.GetHashCode();

        var searchResult = Find(keyTrue);

        int index = searchResult.ind0;

        var node = MasterList[index];

        if(node.Key != keyTrue) throw new Exception($"KEY VALUE '{keyTrue}' DOESN'T EXIST");

        MasterList.RemoveAt(index);

        --Lenght;
    }

    public bool TryRemove (K key)
    {
        int keyTrue = key.GetHashCode();

        var searchResult = Find(keyTrue);

        int index = searchResult.ind0;

        var node = MasterList[index];

        if(node.Key != keyTrue) return false;

        MasterList.RemoveAt(index);

        --Lenght;

        return true;
    }

    public T GetAndRemove (K key)
    {
        int keyTrue = key.GetHashCode();

        var searchResult = Find(keyTrue);

        int index = searchResult.ind0;

        var node = MasterList[index];

        if(node.Key != keyTrue) throw new Exception($"KEY VALUE '{keyTrue}' DOESN'T EXIST");

        MasterList.RemoveAt(index);

        --Lenght;

        return node.Value;
    }

    public bool TryGetAndRemove (K key, out T outValue)
    {
        int keyTrue = key.GetHashCode();

        var searchResult = Find(keyTrue);

        int index = searchResult.ind0;

        var node = MasterList[index];

        if(node.Key != keyTrue)
        {
            outValue = default(T);
            return false;
        }

        MasterList.RemoveAt(index);

        --Lenght;

        outValue = node.Value;

        return true;
    }

    public bool ContainsKey(K key)
    {
        if(Lenght == 0) return false;

        int keyTrue = key.GetHashCode();

        var searchResult = Find(keyTrue);

        int indexFirst = searchResult.ind0;

        return keyTrue == MasterList[indexFirst].Key;
    }

    public bool TryGetValue(K key, out T value)
    {
        int keyTrue = key.GetHashCode();

        if(MasterList.Count == 0)
        {
            value = default(T);

            return false;
        }

        var searchResult = Find(keyTrue);

        int indexFirst = searchResult.ind0;

        int indexLast = searchResult.ind1;

        int indKey = MasterList[indexFirst].Key;

        if(indKey == keyTrue)
        {
            value = MasterList[indexFirst].Value;

            return true;
        }

        value = default(T);

        return false;
    }

    public int GetTrueIndex(K key)
    {
        if(Lenght == 0) return -1;

        int trueKey = key.GetHashCode();

        var search = Find(trueKey);

        var valu = MasterList[search.ind0];

        int toReturn = -1;

        if(valu.Key == trueKey)
            toReturn = search.ind0;

        return toReturn;
    }

    public T GetByTrueIndex(int trueInd)
    {
        return MasterList[trueInd].Value;
    }

    public void SetByTrueIndex(int trueInd, T value)
    {
        MasterList[trueInd] = new KeyValuePair<int, T>(MasterList[trueInd].Key, value);
    }

    public void RemoveByTrueIndex(int trueInd)
    {
        MasterList.RemoveAt(trueInd);
    }

    public int[] GetKeysHash()
    {
        int mc = MasterList.Count;

        int[] arr = new int[mc];

        for(int i = 0; i < mc; ++i)
        {
            arr[i] = MasterList[i].Key;
        }

        return arr;
    }

    public T[] GetValues()
    {
        int count = Count;

        //Most likely gen 1, killed almost immediatly.
        T[] arr = new T[count];

        for(int i = 0; i < count; ++i)
        {
            arr[i] = MasterList[i].Value;
        }

        return arr;
    }

    public List<KeyValuePair<int, T>> GetInternalList()
    {
        return MasterList;
    }

    private (int ind0, int ind1) Find (int key)
    {
        //Search algorithm intended to sqrt the amount
        //of searches.....

        int indexFirst = 0;

        int indexLast = MasterList.Count - 1;

        int leftIndexLast;

        var master = CollectionsMarshal.AsSpan(MasterList);

        loopStart:

        //If index first and index last are the same
        //then we have a 'one index', a scenario
        //where the value belongs either to the right
        //or to the left of the left of the index.
        if(indexFirst == indexLast)
        {
            goto endLoop;
        }

        leftIndexLast = (indexLast - indexFirst) >> 1;
        leftIndexLast += indexFirst;

        bool leftHigher = master[leftIndexLast].Key > key;
        bool rightHigher = master[leftIndexLast+1].Key > key;


        //If both are higher then the result is more to the left,
        //since the results that are not garanteed to be even higher than
        //the subject are there. That makes it become closer to the result.
        //If both are lower, then the result is more to the right,
        //since the results that are not garanteed to be even lower than
        //the subjects there. That makes it become closer to the result.
        //If the left is lower and the higher, then the result belongs in
        //the middle.
        if(leftHigher && rightHigher)
        {
            //CASE: both are higher
            indexLast = leftIndexLast;

            goto loopStart;
        }
        if (leftHigher == rightHigher)
        {
            //CASE: both are lower
            indexFirst = leftIndexLast+1;

            goto loopStart;
        }
        //CASE: left is lower or equal and right is higher
        //aka end of operation.

        indexFirst = leftIndexLast;
        indexLast = leftIndexLast + 1;

        endLoop:

        //Gen 1 allocation, killed almost immediatly.
        return (indexFirst, indexLast);
    }

    public void Clear()
    {
        MasterList.Clear();
        Lenght = 0;
    }

    public int Count => Lenght;
}