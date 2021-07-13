using System;
using System.Collections;
using System.Collections.Generic;
using Pooling;
using UnityEngine;

namespace Dmicade
{
    [RequireComponent(typeof(ContentDisplayManager))]
    public class GridPlaneController : MonoBehaviour
    {
        public GameObject gridAnchor;
        public float planeSpacing;
        [Min(1)] public int planesReps;
        public int planesPerSide = 2;

        //public float resetDistance;

        private Quaternion _gridRotation;
        private Vector3 _gridOrigin;
        private GridPlane[] _planes;
        private Vector3 _forwardScrollDir;
        private ContentDisplayManager _cdm;

        private void Awake()
        {
            _cdm = gameObject.GetComponent<ContentDisplayManager>();
            
            _gridOrigin = gridAnchor.transform.position;
            _gridRotation = gridAnchor.transform.rotation;
            gridAnchor.SetActive(false);
            
            _forwardScrollDir = _cdm.scrollDirection;

            _cdm.OnScrollStart += MoveAllAccelerate;
            _cdm.OnScrollEnd += MoveAllDecelerate;
        }

        void Start()
        {
            InitGrid();
        }

        private void InitGrid()
        {
            _planes = new GridPlane[2 * planesPerSide * planesReps + planesReps];
            int currentPos = -1;
            int planeGroupSize = planesPerSide * 2 + 1;
            Vector3 widthOffset = Vector3.Cross(Vector3.up, _forwardScrollDir.normalized) * planeSpacing;

            for (int i = 0; i < _planes.Length; i++)
            {
                if (i % planeGroupSize == 0) currentPos++;

                _planes[i] = GridPlanePool.Instance.Get();
                _planes[i].gridGroup = currentPos;

                _planes[i].transform.position = _gridOrigin + (planeSpacing * currentPos * _forwardScrollDir) +
                                                ((i % planeGroupSize - planesPerSide) * widthOffset);
                _planes[i].transform.rotation = _gridRotation;

                _planes[i].gameObject.SetActive(true);
            }
        }

        public void MoveAllAccelerate(Vector3 scrollDistance)
        {
            MoveAllPlanes(scrollDistance, _cdm.accelerationType, _cdm.accelerationTime);
        }
        
        public void MoveAllDecelerate(Vector3 scrollDistance)
        {
            MoveAllPlanes(scrollDistance, _cdm.decelerationType, _cdm.decelerationTime);
        }

        private void MoveAllPlanes(Vector3 moveDistance, LeanTweenType easeType, float time, Action callback=null)
        {
            for (int i = 0; i < _planes.Length; i++)
            {
                _planes[i].Move(moveDistance, easeType, time, callback);
            }
        }
    }
}
