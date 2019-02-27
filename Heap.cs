using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heap<T> where T : IHeapItem<T> {
    /*
     *        HEAP EXAMPLE
     *             0
     *          /     \
     *         1       2
     *        / \     / \
     *       3   4   5   6
     *       
     * The parent index must always be less than its children: (n-1) / 2
     *  Child Left is: 2n + 1
     *  Child Right is: 2n + 2
     */

    T[] items;
    int currentItemCount;

    public Heap(int maxHeapSize) {
        items = new T[maxHeapSize];
    }

    public void Add(T item) {
        item.HeapIndex = currentItemCount;
        items[currentItemCount] = item;
        SortUp(item);
        currentItemCount++;
    }

    public T RemoveFirst() {
        T firstItem = items[0];
        currentItemCount--;
        // Take item at the end and put it into the first
        items[0] = items[currentItemCount];
        items[0].HeapIndex = 0;
        // Now sort
        SortDown(items[0]);
        return firstItem;
    }

    public int Count {
        get {
            return currentItemCount;
        }
    }

    public void UpdateItem(T item) {
        SortUp(item);
    }

    public bool Contains(T item) {
        return Equals(items[item.HeapIndex], item);
    }

    void SortUp(T item) {
        int parentIndex = (item.HeapIndex - 1) / 2;
        while (true) {
            T parentItem = items[parentIndex];
            // Use CompareTo here, 
            // as If higher priority, returns 1 | if same priority returns 0 | if lower returns -1
            if (item.CompareTo(parentItem) > 0) {
                //lower f cost
                Swap(item, parentItem);
            }
            else {
                break;
            }

            // Keep recalculating the parent index until we get to where we need
            parentIndex = (item.HeapIndex - 1) / 2;
        }
    }

    void SortDown(T item) {
        while (true) {
            int childIndexLeft = item.HeapIndex * 2 + 1;
            int childInexRight = item.HeapIndex * 2 + 2;
            int swapIndex = 0;
            if (childIndexLeft < currentItemCount) {
                swapIndex = childIndexLeft;

                if (childInexRight < currentItemCount) {
                    // which has higher priority
                    if (items[childIndexLeft].CompareTo(items[childInexRight]) < 0) {
                        swapIndex = childInexRight;
                    }
                }

                if (item.CompareTo(items[swapIndex]) < 0) {
                    Swap(item, items[swapIndex]);
                } else {
                    return;
                }
            }
            else {
                break;
            }
        }
    }

    void Swap(T itemA, T itemB) {
        items[itemA.HeapIndex] = itemB;
        items[itemB.HeapIndex] = itemA;

        int itemAIndex = itemA.HeapIndex;
        itemA.HeapIndex = itemB.HeapIndex;
        itemB.HeapIndex = itemAIndex;
    }
}

public interface IHeapItem<T> : IComparable<T> {
    int HeapIndex {
        get; set;
    }
}