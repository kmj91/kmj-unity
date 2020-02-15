using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityDequeScript
{
    //--------------------------------
    // class
    //--------------------------------

    public class UnityDeque<T>
    {
        //--------------------------------
        // 멤버 변수
        //--------------------------------

        public int Count { get; }   // 사용중인 버퍼 사이즈

        private int m_front;        // 프론트
        private int m_rear;         // 리어
        private T[] m_deque;        // 버퍼

        //--------------------------------
        // 상수
        //--------------------------------

        private const int DEFAULT_SIZE = 10;


        //--------------------------------
        // public 함수
        //--------------------------------

        public UnityDeque()
        {
            m_deque = new T[DEFAULT_SIZE];
        }

        public UnityDeque(int iSize)
        {
            m_deque = new T[iSize];
        }

        ~UnityDeque()
        {

        }

        // 앞
        public bool Push_Front(T data)
        {

            return true;
        }

        public bool Push_Back(T data)
        {

            return true;
        }

        public T Pop_Front()
        {

            return m_deque[m_front];
        }

        public T Pop_Back()
        {

            return m_deque[m_rear];
        }

        public T Peek_Front()
        {

            return m_deque[m_front];
        }

        public T Peek_Back()
        {

            return m_deque[m_rear];
        }

        public void Clear()
        {

        }
    }

}
