﻿using UnityEngine;

namespace PreciseEditor
{
    public static class PartTransform
    {
        public static void SetWorldPosition(Part part, Vector3 newPosition)
        {
            Vector3 localPosition = part.transform.InverseTransformPoint(newPosition);

            part.transform.position = newPosition;
            UpdateEditorGizmo(part);
            GameEvents.onEditorPartEvent.Fire(ConstructionEventType.PartOffset, part);

            if (part.symMethod == SymmetryMethod.Mirror)
            {
                UpdateMirrorSymmetryCounterpartsPosition(part);
            }
            else if (part.symMethod == SymmetryMethod.Radial)
            {
                UpdateRadialSymmetryCounterpartsPosition(part, localPosition);
            }

            EditorLogic.fetch.SetBackup();
        }

        public static void Rotate(Part part, Vector3 eulerAngles, Space space)
        {
            part.transform.Rotate(eulerAngles, space);
            UpdateEditorGizmo(part);
            GameEvents.onEditorPartEvent.Fire(ConstructionEventType.PartRotated, part);

            if (part.symMethod == SymmetryMethod.Mirror)
            {
                RotateMirrorSymmetryCounterparts(part, eulerAngles, space);
            }
            else if (part.symMethod == SymmetryMethod.Radial)
            {
                RotateRadialSymmetryCounterparts(part, eulerAngles, space);
            }

            EditorLogic.fetch.SetBackup();
        }

        private static void UpdateMirrorSymmetryCounterpartsPosition(Part part)
        {
            Vector3 mirrorPosition = MirrorPosition(part.transform.position);

            foreach (Part symmetryCounterpart in part.symmetryCounterparts)
            {
                symmetryCounterpart.transform.position = mirrorPosition;
                UpdateEditorGizmo(symmetryCounterpart);
                GameEvents.onEditorPartEvent.Fire(ConstructionEventType.PartOffset, symmetryCounterpart);
            }
        }

        private static Vector3 MirrorPosition(Vector3 position)
        {
            Vector3 localPosition = position - EditorLogic.RootPart.transform.position;
            Vector3 projection = Vector3.ProjectOnPlane(localPosition, EditorLogic.RootPart.transform.right);
            Vector3 projectedPoint = EditorLogic.RootPart.transform.position + projection;
            Vector3 offset = projectedPoint - position;

            return EditorLogic.RootPart.transform.position + projection + offset;
        }

        private static void UpdateRadialSymmetryCounterpartsPosition(Part part, Vector3 localPosition)
        {
            foreach (Part symmetryCounterpart in part.symmetryCounterparts)
            {
                symmetryCounterpart.transform.Translate(localPosition);
                UpdateEditorGizmo(symmetryCounterpart);
                GameEvents.onEditorPartEvent.Fire(ConstructionEventType.PartOffset, symmetryCounterpart);
            }
        }

        private static void RotateMirrorSymmetryCounterparts(Part part, Vector3 eulerAngles, Space space)
        {
            if (space == Space.Self)
            {
                eulerAngles = part.transform.TransformDirection(eulerAngles);
            }

            eulerAngles = EditorLogic.RootPart.transform.InverseTransformDirection(eulerAngles);
            Quaternion rotation = Quaternion.Euler(eulerAngles);
            rotation.ToAngleAxis(out float angle, out Vector3 axis);
            Vector3 mirrorAxis = new Vector3(-axis.x, axis.y, axis.z);
            mirrorAxis = EditorLogic.RootPart.transform.TransformDirection(mirrorAxis);

            foreach (Part symmetryCounterpart in part.symmetryCounterparts)
            {
                symmetryCounterpart.transform.Rotate(mirrorAxis, -angle, Space.World);
                UpdateEditorGizmo(symmetryCounterpart);
                GameEvents.onEditorPartEvent.Fire(ConstructionEventType.PartRotated, symmetryCounterpart);
            }
        }

        private static void RotateRadialSymmetryCounterparts(Part part, Vector3 eulerAngles, Space space)
        {
            if (space == Space.World)
            {
                eulerAngles = part.transform.InverseTransformDirection(eulerAngles);
            }

            foreach (Part symmetryCounterpart in part.symmetryCounterparts)
            {
                symmetryCounterpart.transform.Rotate(eulerAngles, Space.Self);
                UpdateEditorGizmo(symmetryCounterpart);
                GameEvents.onEditorPartEvent.Fire(ConstructionEventType.PartRotated, symmetryCounterpart);
            }
        }

        private static void UpdateEditorGizmo(Part part)
        {
            if (part == EditorLogic.SelectedPart)
            {
                Vector3 position = part.transform.position;
                Quaternion rotation = part.transform.rotation;

                var gizmoOffset = HighLogic.FindObjectOfType<EditorGizmos.GizmoOffset>();
                if (gizmoOffset != null)
                {
                    gizmoOffset.transform.position = position;
                    if (gizmoOffset.CoordSpace == Space.Self)
                    {
                        gizmoOffset.transform.rotation = rotation;
                    }
                }

                var gizmoRotate = HighLogic.FindObjectOfType<EditorGizmos.GizmoRotate>();
                if (gizmoRotate != null)
                {
                    gizmoRotate.transform.position = position;
                    if (gizmoRotate.CoordSpace == Space.Self)
                    {
                        gizmoRotate.transform.rotation = rotation;
                    }
                }
            }
        }
    }
}