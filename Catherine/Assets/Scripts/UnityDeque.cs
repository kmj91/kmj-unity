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

        public int Count { get; private set; }   // 사용중인 버퍼 사이즈

        private int m_front;        // 프론트
        private int m_rear;         // 리어
        private int m_iBufferSize;  // 버퍼 사이즈
        private T[] m_deque;        // 버퍼

        //--------------------------------
        // 상수
        //--------------------------------

        private const int DEFAULT_SIZE = 11;


        //--------------------------------
        // public 함수
        //--------------------------------

        public UnityDeque()
        {
            m_deque = new T[DEFAULT_SIZE];
            m_iBufferSize = DEFAULT_SIZE;
            m_front = 0;
            m_rear = 0;
        }

        public UnityDeque(int iSize)
        {
            m_deque = new T[iSize];
            m_iBufferSize = iSize;
            m_front = 0;
            m_rear = 0;
        }

        ~UnityDeque()
        {

        }

        // 앞
        public bool Push_Front(T data)
        {
            // 버퍼가 꽉찼나
            if (m_iBufferSize - 1 == Count)
            {
                // 꽉참
                return false;
            }

            if (m_front == 0)
            {
                // 프론트 한칸 왼쪽 이동
                m_front = m_iBufferSize - 1;
                // 데이터 입력
                m_deque[m_front] = data;
                // 사용중인 버퍼 카운트 증가
                ++Count;
            }
            else
            {
                // 프론트 한칸 왼쪽 이동
                m_front = m_front - 1;
                // 데이터 입력
                m_deque[m_front] = data;
                // 사용중인 버퍼 카운트 증가
                ++Count;
            }

            return true;
        }

        public bool Push_Back(T data)
        {
            // 버퍼가 꽉찼나
            if (m_iBufferSize - 1 == Count)
            {
                // 꽉참
                return false;
            }

            // 데이터 입력
            m_deque[m_rear] = data;
            // 사용중인 버퍼 카운트 증가
            ++Count;
            // 리어 한칸 오른쪽 이동
            m_rear = (m_rear + 1) % m_iBufferSize;

            return true;
        }

        public T Pop_Front()
        {
            int iPopFront;

            // 버퍼가 비었나
            if (Count == 0)
            {
                // 빔
                return default(T);
            }

            // 출력할 프론트 인덱스
            iPopFront = m_front;
            // 프론트 한칸 오른쪽 이동
            m_front = (m_front + 1) % m_iBufferSize;
            // 사용중인 버퍼 카운트 감소
            --Count;

            return m_deque[iPopFront];
        }

        public T Pop_Back()
        {
            // 버퍼가 비었나
            if (Count == 0)
            {
                // 빔
                return default(T);
            }

            if (m_rear == 0)
            {
                // 출력할 리어 인덱스
                m_rear = m_iBufferSize - 1;
                // 사용중인 버퍼 카운트 감소
                --Count;
            }
            else
            {
                // 출력할 리어 인덱스
                --m_rear;
                // 사용중인 버퍼 카운트 감소
                --Count;
            }

            return m_deque[m_rear];
        }

        public T Peek_Front()
        {
            // 버퍼가 비었나
            if (Count == 0)
            {
                // 빔
                return default(T);
            }

            return m_deque[m_front];
        }

        public T Peek_Back()
        {
            int iPopRear;

            // 버퍼가 비었나
            if (Count == 0)
            {
                // 빔
                return default(T);
            }

            if (m_rear == 0)
            {
                // 출력할 리어 인덱스
                iPopRear = m_iBufferSize - 1;
            }
            else
            {
                // 출력할 리어 인덱스
                iPopRear = m_rear - 1;
            }

            return m_deque[iPopRear];
        }

        public void Clear()
        {
            Count = 0;
            m_front = 0;
            m_rear = 0;
        }

        public bool isEmpty()
        {
            //if (m_front == m_rear)
            if (Count == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

}
