using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dmicade
{
    [RequireComponent(typeof(ContentDisplayManager))]
    public class GridOffsetController : MonoBehaviour
    {
        public GameObject topGrid;
        public GameObject bottomGrid;

        public float moveFactor = 1.0f;

        private ContentDisplayManager _cdm;
        private Material _staticTopGridMaterial; 
        private Material _staticBottomGridMaterial;
        private int _offsetShaderId;
        private Vector4 _topGridOffset;
        private Vector4 _bottomGridOffset;
        private float _test = 0.0f;
        

        private void Awake()
        {
            _cdm = gameObject.GetComponent<ContentDisplayManager>();

            _cdm.OnScrollStart += MoveAccelerate;
            _cdm.OnScrollEnd += MoveDecelerate;

            _staticBottomGridMaterial = bottomGrid.GetComponent<Renderer>().material;
            _staticTopGridMaterial = topGrid.GetComponent<Renderer>().material;
            
            //_staticBottomGridMaterial.EnableKeyword("_Offset");
            _offsetShaderId = Shader.PropertyToID("_Offset");
        }

        private void MoveAccelerate(Vector3 distance) =>
            MoveGrids(distance, _cdm.accelerationTime, _cdm.accelerationType);

        private void MoveDecelerate(Vector3 distance) =>
            MoveGrids(distance, _cdm.decelerationTime, _cdm.decelerationType);

        private void MoveGrids(Vector3 distance, float duration, LeanTweenType easeType) 
        {
            Vector2 offsetStartBottom = _staticBottomGridMaterial.GetVector(_offsetShaderId);
            Vector2 offsetStartTop = _staticTopGridMaterial.GetVector(_offsetShaderId);
            Vector2 destBottom = offsetStartBottom + new Vector2(distance.x, distance.z) * moveFactor;
            Vector2 destTop = offsetStartTop + new Vector2(-distance.x, distance.z) * moveFactor;

            LeanTween.value(gameObject, MoveBottomGrid, offsetStartBottom, destBottom, duration).setEase(easeType);
            LeanTween.value(gameObject, MoveTopGrid, offsetStartTop, destTop, duration).setEase(easeType);
        }
        private void MoveBottomGrid(Vector2 offset) => _staticBottomGridMaterial.SetVector(_offsetShaderId, offset);

        private void MoveTopGrid(Vector2 offset) => _staticTopGridMaterial.SetVector(_offsetShaderId, offset);
        
    }
    
}
