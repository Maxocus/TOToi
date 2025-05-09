﻿// Magica Cloth 2.
// Copyright (c) 2023 MagicaSoft.
// https://magicasoft.jp
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace MagicaCloth2
{
    /// <summary>
    /// コライダーの管理
    /// MC1と違いコライダーはパーティクルとは別に管理される
    /// コライダーはチームごとに分けて管理される。
    /// 同じコライダーをチームAとチームBが共有していたとしてもそれぞれ別のコライダーとして登録される。
    /// </summary>
    public class ColliderManager : IManager, IValid
    {
        /// <summary>
        /// チームID
        /// </summary>
        public ExNativeArray<short> teamIdArray;

        /// <summary>
        /// コライダー種類(最大15まで)
        /// </summary>
        public enum ColliderType : byte
        {
            None = 0,
            Sphere = 1,
            CapsuleX_Center = 2,
            CapsuleY_Center = 3,
            CapsuleZ_Center = 4,
            CapsuleX_Start = 5,
            CapsuleY_Start = 6,
            CapsuleZ_Start = 7,
            Plane = 8,
            Box = 9,
        }

        /// <summary>
        /// フラグ(8bit)
        /// 下位4bitはコライダー種類
        /// 上位4bitはフラグ
        /// </summary>
        public const byte Flag_Valid = 0x10; // データの有無
        public const byte Flag_Enable = 0x20; // 有効状態
        public const byte Flag_Reset = 0x40; // 位置リセット
        public const byte Flag_Reverse = 0x80; // 方向逆転
        public ExNativeArray<ExBitFlag8> flagArray;

        /// <summary>
        /// トランスフォームからの中心ローカルオフセット位置
        /// </summary>
        public ExNativeArray<float3> centerArray;

        /// <summary>
        /// コライダーのサイズ情報
        /// Sphere(x:半径)
        /// Capsule(x:始点半径, y:終点半径, z:長さ)
        /// Box(x:サイズX, y:サイズY, z:サイズZ)
        /// </summary>
        public ExNativeArray<float3> sizeArray;

        /// <summary>
        /// 現フレーム姿勢
        /// トランスフォームからスナップされたチームローカル姿勢
        /// センターオフセットも計算される
        /// </summary>
        public ExNativeArray<float3> framePositions;
        public ExNativeArray<quaternion> frameRotations;
        public ExNativeArray<float3> frameScales;

        /// <summary>
        /// １つ前のフレーム姿勢
        /// </summary>
        public ExNativeArray<float3> oldFramePositions;
        public ExNativeArray<quaternion> oldFrameRotations;
        //public ExNativeArray<float3> oldFrameScales;

        /// <summary>
        /// 現ステップでの姿勢
        /// </summary>
        public ExNativeArray<float3> nowPositions;
        public ExNativeArray<quaternion> nowRotations;
        //public ExNativeArray<float3> nowScales;

        public ExNativeArray<float3> oldPositions;
        public ExNativeArray<quaternion> oldRotations;


        /// <summary>
        /// 有効なコライダーデータ数
        /// </summary>
        public int DataCount => teamIdArray?.Count ?? 0;

        /// <summary>
        /// 登録コライダーコンポーネント
        /// </summary>
        public HashSet<ColliderComponent> colliderSet = new HashSet<ColliderComponent>();

        /// <summary>
        /// 登録コライダー数
        /// </summary>
        public int ColliderCount => colliderSet.Count;

        bool isValid = false;

        //=========================================================================================
        /// <summary>
        /// ステップごとの作業データ
        /// </summary>
        internal struct WorkData
        {
            public AABB aabb;
            public float2 radius;
            public float3x2 oldPos;
            public float3x2 nextPos;
            public quaternion inverseOldRot;
            public quaternion rot;
        }

        internal ExNativeArray<WorkData> workDataArray;

        //=========================================================================================
        public void Dispose()
        {
            isValid = false;

            teamIdArray?.Dispose();
            flagArray?.Dispose();
            centerArray?.Dispose();
            sizeArray?.Dispose();
            framePositions?.Dispose();
            frameRotations?.Dispose();
            frameScales?.Dispose();
            nowPositions?.Dispose();
            nowRotations?.Dispose();
            //nowScales?.Dispose();
            oldFramePositions?.Dispose();
            oldFrameRotations?.Dispose();
            //oldFrameScales?.Dispose();
            oldPositions?.Dispose();
            oldRotations?.Dispose();
            workDataArray?.Dispose();

            teamIdArray = null;
            flagArray = null;
            sizeArray = null;
            framePositions = null;
            frameRotations = null;
            frameScales = null;
            nowPositions = null;
            nowRotations = null;
            //nowScales = null;
            oldFramePositions = null;
            oldFrameRotations = null;
            //oldFrameScales = null;
            oldPositions = null;
            oldRotations = null;
            workDataArray = null;

            colliderSet.Clear();
        }

        public void EnterdEditMode()
        {
            Dispose();
        }

        public void Initialize()
        {
            Dispose();

            const int capacity = 256;
            teamIdArray = new ExNativeArray<short>(capacity);
            flagArray = new ExNativeArray<ExBitFlag8>(capacity);
            centerArray = new ExNativeArray<float3>(capacity);
            sizeArray = new ExNativeArray<float3>(capacity);
            framePositions = new ExNativeArray<float3>(capacity);
            frameRotations = new ExNativeArray<quaternion>(capacity);
            frameScales = new ExNativeArray<float3>(capacity);
            nowPositions = new ExNativeArray<float3>(capacity);
            nowRotations = new ExNativeArray<quaternion>(capacity);
            //nowScales = new ExNativeArray<float3>(capacity);
            oldFramePositions = new ExNativeArray<float3>(capacity);
            oldFrameRotations = new ExNativeArray<quaternion>(capacity);
            //oldFrameScales = new ExNativeArray<float3>(capacity);
            oldPositions = new ExNativeArray<float3>(capacity);
            oldRotations = new ExNativeArray<quaternion>(capacity);
            workDataArray = new ExNativeArray<WorkData>(capacity);

            isValid = true;
        }

        public bool IsValid()
        {
            return isValid;
        }

        //=========================================================================================
        /// <summary>
        /// チームにコライダー領域を登録する
        /// 最初から最大コライダー数で領域を初期化しておく
        /// </summary>
        /// <param name="cprocess"></param>
        public void Register(ClothProcess cprocess)
        {
            if (isValid == false)
                return;

            // コライダー数
            int cnt = cprocess.cloth.SerializeData.colliderCollisionConstraint.ColliderLength;
            if (cnt > 0)
            {
                int teamId = cprocess.TeamId;
                ref var tdata = ref MagicaManager.Team.GetTeamDataRef(teamId);

                // コライダー数で初期化
                tdata.colliderChunk = teamIdArray.AddRange(cnt, (short)teamId);
                flagArray.AddRange(cnt, default);
                centerArray.AddRange(cnt);
                sizeArray.AddRange(cnt);
                framePositions.AddRange(cnt);
                frameRotations.AddRange(cnt);
                frameScales.AddRange(cnt);
                nowPositions.AddRange(cnt);
                nowRotations.AddRange(cnt);
                //nowScales.AddRange(cnt);
                oldFramePositions.AddRange(cnt);
                oldFrameRotations.AddRange(cnt);
                //oldFrameScales.AddRange(cnt);
                oldPositions.AddRange(cnt);
                oldRotations.AddRange(cnt);
                workDataArray.AddRange(cnt);
                tdata.colliderTransformChunk = MagicaManager.Bone.AddTransform(cnt, teamId); // 領域のみ
                tdata.colliderCount = 0;
                cprocess.colliderList.AddRange(new ColliderComponent[cnt]); // nullで領域確保

                // 初期コライダー登録
                InitColliders(cprocess);
            }
        }

        /// <summary>
        /// チームからコライダー領域を解除する
        /// </summary>
        /// <param name="cprocess"></param>
        public void Exit(ClothProcess cprocess)
        {
            if (isValid == false)
                return;

            int teamId = cprocess.TeamId;

            // コライダー解除
            foreach (var col in cprocess.colliderList)
            {
                if (col)
                {
                    col.Exit(teamId);
                    colliderSet.Remove(col);
                }
            }
            cprocess.colliderList.Clear();

            ref var tdata = ref MagicaManager.Team.GetTeamDataRef(teamId);

            var c = tdata.colliderChunk;
            teamIdArray.RemoveAndFill(c); // 0クリア
            flagArray.RemoveAndFill(c); // 0クリア
            centerArray.Remove(c);
            sizeArray.Remove(c);
            framePositions.Remove(c);
            frameRotations.Remove(c);
            frameScales.Remove(c);
            nowPositions.Remove(c);
            nowRotations.Remove(c);
            //nowScales.Remove(c);
            oldFramePositions.Remove(c);
            oldFrameRotations.Remove(c);
            //oldFrameScales.Remove(c);
            oldPositions.Remove(c);
            oldRotations.Remove(c);
            workDataArray.Remove(c);

            tdata.colliderChunk.Clear();
            tdata.colliderCount = 0;

            // コライダートランスフォーム解除
            MagicaManager.Bone.RemoveTransform(tdata.colliderTransformChunk);
            tdata.colliderTransformChunk.Clear();
        }

        //=========================================================================================
        /// <summary>
        /// 初期コライダーの登録
        /// </summary>
        /// <param name="cprocess"></param>
        internal void InitColliders(ClothProcess cprocess)
        {
            var clist = cprocess.cloth.SerializeData.colliderCollisionConstraint.colliderList;
            if (clist.Count == 0)
                return;

            ref var tdata = ref MagicaManager.Team.GetTeamDataRef(cprocess.TeamId);
            int index = 0;
            for (int i = 0; i < clist.Count; i++)
            {
                var col = clist[i];
                if (col && cprocess.colliderList.Contains(col) == false)
                {
                    AddColliderInternal(cprocess, col, index, tdata.colliderChunk.startIndex + index, tdata.colliderTransformChunk.startIndex + index);
                    tdata.colliderCount++;
                    index++;
                }
            }
        }

        /// <summary>
        /// コライダーの内容を更新する
        /// </summary>
        /// <param name="cprocess"></param>
        internal void UpdateColliders(ClothProcess cprocess)
        {
            if (isValid == false)
                return;
            var clist = cprocess.cloth.SerializeData.colliderCollisionConstraint.colliderList;

            // 現在の登録コライダーと比較し削除と追加を行う
            // 既存削除
            for (int i = 0, cnt = cprocess.ColliderCapacity; i < cnt;)
            {
                var col = cprocess.colliderList[i];
                if (col && clist.Contains(col) == false)
                {
                    // コライダー消滅
                    RemoveCollider(col, cprocess.TeamId);
                }
                else
                    i++;
            }

            // 新規追加
            foreach (var col in clist)
            {
                if (col && cprocess.GetColliderIndex(col) < 0)
                {
                    AddCollider(cprocess, col);
                    //Debug.Log($"コライダー追加:{col.name}");
                }
            }
        }

        /// <summary>
        /// コライダーの個別登録
        /// </summary>
        /// <param name="cprocess"></param>
        /// <param name="col"></param>
        void AddCollider(ClothProcess cprocess, ColliderComponent col)
        {
            if (isValid == false)
                return;
            if (col == null)
                return;
            if (cprocess.GetColliderIndex(col) >= 0)
                return; // すでに追加済み

            ref var tdata = ref MagicaManager.Team.GetTeamDataRef(cprocess.TeamId);
            Debug.Assert(tdata.IsValid);

            if (tdata.colliderChunk.IsValid == false)
            {
                // 新規確保
                int newCount = Define.System.ExpandedColliderCount;
                tdata.colliderChunk = teamIdArray.AddRange(newCount, (short)cprocess.TeamId);
                flagArray.AddRange(newCount, default);
                centerArray.AddRange(newCount);
                sizeArray.AddRange(newCount);
                framePositions.AddRange(newCount);
                frameRotations.AddRange(newCount);
                frameScales.AddRange(newCount);
                nowPositions.AddRange(newCount);
                nowRotations.AddRange(newCount);
                oldFramePositions.AddRange(newCount);
                oldFrameRotations.AddRange(newCount);
                oldPositions.AddRange(newCount);
                oldRotations.AddRange(newCount);
                workDataArray.AddRange(newCount);
                tdata.colliderTransformChunk = MagicaManager.Bone.AddTransform(newCount, cprocess.TeamId); // 領域のみ
                tdata.colliderCount = 0;
                cprocess.colliderList.AddRange(new ColliderComponent[newCount]); // nullで領域確保
            }
            else if (tdata.ColliderCount == tdata.colliderChunk.dataLength)
            {
                // コライダー配列のキャパシティ上限なら拡張する
                // 拡張
                int newCount = tdata.colliderChunk.dataLength + Define.System.ExpandedColliderCount;
                var oldColliderChunk = tdata.colliderChunk;
                tdata.colliderChunk = teamIdArray.Expand(oldColliderChunk, newCount);
                flagArray.ExpandAndFill(oldColliderChunk, newCount); // 旧領域のフラグはクリアする必要あり
                centerArray.Expand(oldColliderChunk, newCount);
                sizeArray.Expand(oldColliderChunk, newCount);
                framePositions.Expand(oldColliderChunk, newCount);
                frameRotations.Expand(oldColliderChunk, newCount);
                frameScales.Expand(oldColliderChunk, newCount);
                nowPositions.Expand(oldColliderChunk, newCount);
                nowRotations.Expand(oldColliderChunk, newCount);
                oldFramePositions.Expand(oldColliderChunk, newCount);
                oldFrameRotations.Expand(oldColliderChunk, newCount);
                oldPositions.Expand(oldColliderChunk, newCount);
                oldRotations.Expand(oldColliderChunk, newCount);
                workDataArray.Expand(oldColliderChunk, newCount);

                // コライダートランスフォーム拡張
                var oldColliderTransformChunk = tdata.colliderTransformChunk;
                tdata.colliderTransformChunk = MagicaManager.Bone.Expand(oldColliderTransformChunk, newCount);

                // コライダー配列拡張
                cprocess.colliderList.AddRange(new ColliderComponent[Define.System.ExpandedColliderCount]);
            }

            // 最後に追加する
            int index = tdata.colliderCount;
            int arrayIndex = tdata.colliderChunk.startIndex + index;
            int transformIndex = tdata.colliderTransformChunk.startIndex + index;
            AddColliderInternal(cprocess, col, index, arrayIndex, transformIndex);

            tdata.colliderCount++;
        }

        /// <summary>
        /// コライダーを削除する
        /// ここでは領域は削除せずにデータのみを無効化させる
        /// 領域は生存する最後尾のデータと入れ替えられる(SwapBack)
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="localIndex"></param>
        internal void RemoveCollider(ColliderComponent col, int teamId)
        {
            if (isValid == false)
                return;
            ref var tdata = ref MagicaManager.Team.GetTeamDataRef(teamId);
            int ccnt = tdata.colliderCount;
            if (ccnt == 0)
                return;
            var cprocess = MagicaManager.Team.GetClothProcess(teamId);
            if (cprocess == null)
                return;
            int index = cprocess.GetColliderIndex(col);
            if (index < 0)
                return;

            int arrayIndex = tdata.colliderChunk.startIndex + index;
            int transformIndex = tdata.colliderTransformChunk.startIndex + index;

            int swapIndex = ccnt - 1;
            int swapArrayIndex = tdata.colliderChunk.startIndex + swapIndex;
            int swapTransformIndex = tdata.colliderTransformChunk.startIndex + swapIndex;

            if (arrayIndex < swapArrayIndex)
            {
                // remove swap back
                flagArray[arrayIndex] = flagArray[swapArrayIndex];
                teamIdArray[arrayIndex] = teamIdArray[swapArrayIndex];
                centerArray[arrayIndex] = centerArray[swapArrayIndex];
                sizeArray[arrayIndex] = sizeArray[swapArrayIndex];
                framePositions[arrayIndex] = framePositions[swapArrayIndex];
                frameRotations[arrayIndex] = frameRotations[swapArrayIndex];
                frameScales[arrayIndex] = frameScales[swapArrayIndex];
                nowPositions[arrayIndex] = nowPositions[swapArrayIndex];
                nowRotations[arrayIndex] = nowRotations[swapArrayIndex];
                //nowScales[arrayIndex] = nowScales[swapArrayIndex];
                oldFramePositions[arrayIndex] = oldFramePositions[swapArrayIndex];
                oldFrameRotations[arrayIndex] = oldFrameRotations[swapArrayIndex];
                //oldFrameScales[arrayIndex] = oldFrameScales[swapArrayIndex];
                oldPositions[arrayIndex] = oldPositions[swapArrayIndex];
                oldRotations[arrayIndex] = oldRotations[swapArrayIndex];

                flagArray[swapArrayIndex] = default;
                teamIdArray[swapArrayIndex] = 0;

                // transform
                MagicaManager.Bone.CopyTransform(swapTransformIndex, transformIndex);
                MagicaManager.Bone.SetTransform(null, default, swapTransformIndex, 0);

                // cprocess
                cprocess.colliderList[index] = cprocess.colliderList[swapIndex];
                cprocess.colliderList[swapIndex] = null;
            }
            else
            {
                // remove
                flagArray[arrayIndex] = default;
                teamIdArray[arrayIndex] = 0;

                // transform
                MagicaManager.Bone.SetTransform(null, default, transformIndex, 0);

                // cprocess
                cprocess.colliderList[index] = null;
            }

            tdata.colliderCount--;

            colliderSet.Remove(col);
        }

        void AddColliderInternal(ClothProcess cprocess, ColliderComponent col, int index, int arrayIndex, int transformIndex)
        {
            int teamId = cprocess.TeamId;

            // マネージャへ登録
            teamIdArray[arrayIndex] = (short)teamId;
            var flag = new ExBitFlag8();
            flag = DataUtility.SetColliderType(flag, col.GetColliderType());
            flag.SetFlag(Flag_Valid, true);
            flag.SetFlag(Flag_Enable, col.isActiveAndEnabled);
            flag.SetFlag(Flag_Reset, true);
            flag.SetFlag(Flag_Reverse, col.IsReverseDirection());
            flagArray[arrayIndex] = flag;
            centerArray[arrayIndex] = col.center;
            sizeArray[arrayIndex] = col.GetSize();
            var pos = col.transform.position;
            var rot = col.transform.rotation;
            var scl = col.transform.localScale;
            framePositions[arrayIndex] = pos;
            frameRotations[arrayIndex] = rot;
            frameScales[arrayIndex] = scl;
            nowPositions[arrayIndex] = pos;
            nowRotations[arrayIndex] = rot;
            //nowScales[arrayIndex] = scl;
            oldFramePositions[arrayIndex] = pos;
            oldFrameRotations[arrayIndex] = rot;
            //oldFrameScales[arrayIndex] = scl;
            oldPositions[arrayIndex] = pos;
            oldRotations[arrayIndex] = rot;

            // チームにコライダーコンポーネントを登録
            cprocess.colliderList[index] = col;

            // コライダーコンポーネント側にも登録する
            col.Register(teamId);

            // トランスフォーム登録
            bool t_enable = cprocess.IsEnable && flag.IsSet(Flag_Enable);
            var tflag = new ExBitFlag8(TransformManager.Flag_Read);
            tflag.SetFlag(TransformManager.Flag_Enable, t_enable);
            MagicaManager.Bone.SetTransform(col.transform, tflag, transformIndex, teamId);

            colliderSet.Add(col);
        }

        /// <summary>
        /// 有効状態の変更
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="index"></param>
        /// <param name="sw"></param>
        internal void EnableCollider(ColliderComponent col, int teamId, bool sw)
        {
            if (IsValid() == false)
                return;
            ref var tdata = ref MagicaManager.Team.GetTeamDataRef(teamId);
            if (tdata.IsValid == false)
                return;
            var cprocess = MagicaManager.Team.GetClothProcess(teamId);
            int index = cprocess.GetColliderIndex(col);
            if (index < 0)
                return;
            int arrayIndex = tdata.colliderChunk.startIndex + index;
            var flag = flagArray[arrayIndex];
            flag.SetFlag(Flag_Enable, sw);
            flag.SetFlag(Flag_Reset, true); // Enable/Disableどちらでもリセット
            flagArray[arrayIndex] = flag;

            // トランスフォーム有効状態
            int transformIndex = tdata.colliderTransformChunk.startIndex + index;
            bool t_enable = cprocess.IsEnable && flag.IsSet(Flag_Enable);
            MagicaManager.Bone.EnableTransform(transformIndex, t_enable);
        }

        /// <summary>
        /// チーム有効状態変更に伴うコライダー状態の変更
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="sw"></param>
        internal void EnableTeamCollider(int teamId)
        {
            if (IsValid() == false)
                return;
            ref var tdata = ref MagicaManager.Team.GetTeamDataRef(teamId);
            if (tdata.IsValid == false)
                return;
            if (tdata.ColliderCount == 0)
                return;

            bool teamEnable = tdata.IsEnable;
            var c = tdata.colliderTransformChunk;
            for (int i = 0; i < c.dataLength; i++)
            {
                int arrayIndex = tdata.colliderChunk.startIndex + i;
                int transformIndex = c.startIndex + i;

                // フラグ
                var flag = flagArray[arrayIndex];
                //flag.SetFlag(Flag_Enable, sw); // コライダーのEnableフラグはコライダー固有のものなので変更する必要はない
                flag.SetFlag(Flag_Reset, true); // Enable/Disableどちらでもリセット
                flagArray[arrayIndex] = flag;

                // 有効状態
                bool t_enable = teamEnable && flag.IsSet(Flag_Enable);
                MagicaManager.Bone.EnableTransform(transformIndex, t_enable);
            }
        }

        /// <summary>
        /// コライダーコンポーネントのパラメータ変更を反映する
        /// </summary>
        /// <param name="col"></param>
        /// <param name="teamId"></param>
        internal void UpdateParameters(ColliderComponent col, int teamId)
        {
            if (IsValid() == false)
                return;

            ref var tdata = ref MagicaManager.Team.GetTeamDataRef(teamId);
            if (tdata.IsValid == false)
                return;
            var cprocess = MagicaManager.Team.GetClothProcess(teamId);
            int index = cprocess.GetColliderIndex(col);
            if (index < 0)
                return;
            int arrayIndex = tdata.colliderChunk.startIndex + index;

            var flag = flagArray[arrayIndex];
            flag = DataUtility.SetColliderType(flag, col.GetColliderType());
            flag.SetFlag(Flag_Reverse, col.IsReverseDirection());
            flagArray[arrayIndex] = flag;
            centerArray[arrayIndex] = col.center;
            sizeArray[arrayIndex] = math.max(col.GetSize(), 0.0001f); // 念のため
        }

        //=========================================================================================
        // Simulation
        //=========================================================================================
        /// <summary>
        /// シミュレーション更新前処理
        /// コライダー姿勢の読み取り
        /// </summary>
        internal static void SimulationPreUpdate(
            DataChunk chunk,
            // team
            ref TeamManager.TeamData tdata,
            ref InertiaConstraint.CenterData cdata,
            // collider
            //public NativeArray<short> teamIdArray;
            ref NativeArray<ExBitFlag8> flagArray,
            ref NativeArray<float3> centerArray,
            ref NativeArray<float3> framePositions,
            ref NativeArray<quaternion> frameRotations,
            ref NativeArray<float3> frameScales,
            ref NativeArray<float3> oldFramePositions,
            ref NativeArray<quaternion> oldFrameRotations,
            ref NativeArray<float3> nowPositions,
            ref NativeArray<quaternion> nowRotations,
            ref NativeArray<float3> oldPositions,
            ref NativeArray<quaternion> oldRotations,
            // transform (ワールド姿勢)
            ref NativeArray<float3> transformPositionArray,
            ref NativeArray<quaternion> transformRotationArray,
            ref NativeArray<float3> transformScaleArray
            )
        {
            // コライダーごと
            //int index = tdata.colliderChunk.startIndex;
            int index = tdata.colliderChunk.startIndex + chunk.startIndex;
            //for (int i = 0; i < tdata.colliderChunk.dataLength; i++, index++)
            for (int i = 0; i < chunk.dataLength; i++, index++)
            {
                var flag = flagArray[index];
                if (flag.IsSet(Flag_Valid) == false || flag.IsSet(Flag_Enable) == false)
                    continue;

                var center = centerArray[index];
                int l_index = index - tdata.colliderChunk.startIndex;
                int t_index = tdata.colliderTransformChunk.startIndex + l_index;

                // transform姿勢（ワールド）
                var wpos = transformPositionArray[t_index];
                var wrot = transformRotationArray[t_index];
                var wscl = transformScaleArray[t_index];

                // オフセット
                wpos += math.mul(wrot, center) * wscl;

                // 格納
                framePositions[index] = wpos;
                frameRotations[index] = wrot;
                frameScales[index] = wscl;

                //Debug.Log($"C wpos:{wpos}");
                //Debug.Log($"C wrot:{wrot}");
                //Debug.Log($"C wscl:{wscl}");

                // リセット処理
                if (tdata.IsReset || flag.IsSet(Flag_Reset))
                {
                    oldFramePositions[index] = wpos;
                    oldFrameRotations[index] = wrot;
                    //oldFrameScales[index] = lscl;
                    nowPositions[index] = wpos;
                    nowRotations[index] = wrot;
                    oldPositions[index] = wpos;
                    oldRotations[index] = wrot;

                    flag.SetFlag(Flag_Reset, false);
                    flagArray[index] = flag;
                }
                else if (tdata.IsInertiaShift || tdata.IsNegativeScaleTeleport)
                {
                    // 慣性全体シフト
                    var oldFramePosition = oldFramePositions[index];
                    var oldFrameRotation = oldFrameRotations[index];
                    var nowPosition = nowPositions[index];
                    var nowRotation = nowRotations[index];
                    var oldPosition = oldPositions[index];
                    var oldRotation = oldRotations[index];

                    // マイナススケール
                    if (tdata.IsNegativeScaleTeleport)
                    {
                        // 本体のスケール反転に合わせてシミュレーションに影響が出ないように必要な座標系を同様に軸反転させる
                        // コライダーはセンター空間で反転させる
                        // 回転に関してはパーティクルとは異なり法線接線をスケール方向により反転させて組み直す

                        // センター空間軸反転用マトリックス
                        float4x4 negativeM = cdata.negativeScaleMatrix;

                        oldFramePosition = MathUtility.TransformPoint(oldFramePosition, negativeM);
                        oldFrameRotation = MathUtility.TransformRotation(oldFrameRotation, negativeM, tdata.negativeScaleChange);

                        nowPosition = MathUtility.TransformPoint(nowPosition, negativeM);
                        nowRotation = MathUtility.TransformRotation(nowRotation, negativeM, tdata.negativeScaleChange);

                        oldPosition = MathUtility.TransformPoint(oldPosition, negativeM);
                        oldRotation = MathUtility.TransformRotation(oldRotation, negativeM, tdata.negativeScaleChange);
                    }

                    if (tdata.IsInertiaShift)
                    {
                        // cdata.frameComponentShiftVector : 全体シフトベクトル
                        // cdata.frameComponentShiftRotation : 全体シフト回転
                        // cdata.oldComponentWorldPosition : フレーム移動前のコンポーネント中心位置

                        float3 prevFrameWorldPosition = cdata.oldComponentWorldPosition;

                        oldFramePosition = MathUtility.ShiftPosition(oldFramePosition, prevFrameWorldPosition, cdata.frameComponentShiftVector, cdata.frameComponentShiftRotation);
                        oldFrameRotation = math.mul(cdata.frameComponentShiftRotation, oldFrameRotation);

                        nowPosition = MathUtility.ShiftPosition(nowPosition, prevFrameWorldPosition, cdata.frameComponentShiftVector, cdata.frameComponentShiftRotation);
                        nowRotation = math.mul(cdata.frameComponentShiftRotation, nowRotation);

                        oldPosition = MathUtility.ShiftPosition(oldPosition, prevFrameWorldPosition, cdata.frameComponentShiftVector, cdata.frameComponentShiftRotation);
                        oldRotation = math.mul(cdata.frameComponentShiftRotation, oldRotation);
                    }

                    oldFramePositions[index] = oldFramePosition;
                    oldFrameRotations[index] = oldFrameRotation;
                    nowPositions[index] = nowPosition;
                    nowRotations[index] = nowRotation;
                    oldPositions[index] = oldPosition;
                    oldRotations[index] = oldRotation;
                }
            }
        }

        internal static void SimulationStartStep(
            // team
            ref TeamManager.TeamData tdata,
            ref InertiaConstraint.CenterData cdata,
            // collider
            ref NativeArray<ExBitFlag8> flagArray,
            ref NativeArray<float3> sizeArray,
            ref NativeArray<float3> framePositions,
            ref NativeArray<quaternion> frameRotations,
            ref NativeArray<float3> frameScales,
            ref NativeArray<float3> oldFramePositions,
            ref NativeArray<quaternion> oldFrameRotations,
            ref NativeArray<float3> nowPositions,
            ref NativeArray<quaternion> nowRotations,
            ref NativeArray<float3> oldPositions,
            ref NativeArray<quaternion> oldRotations,
            ref NativeArray<WorkData> workDataArray
            )
        {
            // コライダーごと
            int cindex = tdata.colliderChunk.startIndex;
            for (int i = 0; i < tdata.colliderChunk.dataLength; i++, cindex++)
            {
                var flag = flagArray[cindex];
                if (flag.IsSet(Flag_Valid) == false || flag.IsSet(Flag_Enable) == false)
                    continue;

                // 今回のシミュレーションステップでの姿勢を求める
                float3 pos = math.lerp(oldFramePositions[cindex], framePositions[cindex], tdata.frameInterpolation);
                quaternion rot = math.slerp(oldFrameRotations[cindex], frameRotations[cindex], tdata.frameInterpolation);
                rot = math.normalize(rot); // 必要
                nowPositions[cindex] = pos;
                nowRotations[cindex] = rot;
                //Debug.Log($"cpos:{pos}, coldpos:{oldPos}");

                // コライダー慣性シフト
                // old姿勢をシフトさせる
                var oldpos = oldPositions[cindex];
                var oldrot = oldRotations[cindex];

                // ローカル慣性シフト
                oldpos = math.lerp(oldpos, pos, cdata.stepMoveInertiaRatio);
                oldrot = math.slerp(oldrot, rot, cdata.stepRotationInertiaRatio);
                oldPositions[cindex] = oldpos;
                oldRotations[cindex] = math.normalize(oldrot);

                // ステップ作業データの構築
                var type = DataUtility.GetColliderType(flag);
                var work = new WorkData();
                var csize = sizeArray[cindex];
                var cscl = frameScales[cindex];
                work.inverseOldRot = math.inverse(oldrot);
                work.rot = rot;
                if (type == ColliderType.Sphere)
                {
                    // radius
                    float radius = csize.x * math.abs(cscl.x); // X軸のみを見る
                    work.radius = radius;

                    // aabb
                    var aabb = new AABB(math.min(oldpos, pos), math.max(oldpos, pos));
                    aabb.Expand(radius);
                    work.aabb = aabb;

                    // oldpos
                    work.oldPos.c0 = oldpos;

                    // nextpos
                    work.nextPos.c0 = pos;
                }
                else if (type >= ColliderType.CapsuleX_Center && type <= ColliderType.CapsuleZ_Start)
                {
                    // 中央揃え
                    bool alignedCenter = type >= ColliderType.CapsuleX_Center && type <= ColliderType.CapsuleZ_Center;

                    // 方向性
                    float3 dir = (type == ColliderType.CapsuleX_Center || type == ColliderType.CapsuleX_Start) ? math.right()
                        : (type == ColliderType.CapsuleY_Center || type == ColliderType.CapsuleY_Start) ? math.up()
                        : math.forward();

                    // スケール
                    //float scl = math.dot(math.abs(cscl), dir); // dirの軸のスケールを使用する

                    // マイナススケール
                    float scl0 = math.dot(cscl, dir); // dirの軸のスケールを使用する
                    dir *= math.sign(scl0); // 方向反転
                    float scl = math.abs(scl0);

                    // 逆方向
                    if (flag.IsSet(Flag_Reverse))
                        dir = -dir;

                    // x = 始点半径
                    // y = 終点半径
                    // z = 長さ
                    csize *= scl;

                    float sr = csize.x;
                    float er = csize.y;
                    float length = csize.z;

                    // 長さ
                    float slen = alignedCenter ? length * 0.5f : 0.0f;
                    float elen = alignedCenter ? length * 0.5f : (length - sr);
                    slen = math.max(slen - sr, 0.0f);
                    elen = math.max(elen - er, 0.0f);

                    // 移動前カプセル始点と終点
                    float3 soldpos = oldpos + math.mul(oldrot, dir * slen);
                    float3 eoldpos = oldpos - math.mul(oldrot, dir * elen);

                    // 移動後カプセル始点と終点
                    float3 spos = pos + math.mul(rot, dir * slen);
                    float3 epos = pos - math.mul(rot, dir * elen);

                    // AABB
                    var aabbC = new AABB(math.min(soldpos, spos) - sr, math.max(soldpos, spos) + sr);
                    var aabbC1 = new AABB(math.min(eoldpos, epos) - er, math.max(eoldpos, epos) + er);
                    aabbC.Encapsulate(aabbC1);

                    // 格納
                    work.aabb = aabbC;
                    work.radius = new float2(sr, er);
                    work.oldPos = new float3x2(soldpos, eoldpos);
                    work.nextPos = new float3x2(spos, epos);
                }
                else if (type == ColliderType.Plane)
                {
                    // 押し出し法線方向をoldposに格納する
                    // マイナススケール
                    float3 dir = math.up();
                    dir *= math.sign(cscl.y); // Y反転時は逆にする

                    float3 n = math.mul(rot, dir);
                    work.oldPos.c0 = n;
                    work.nextPos.c0 = pos;
                }

                workDataArray[cindex] = work;
            }
        }

        /// <summary>
        /// シミュレーションステップ後処理
        /// old姿勢の格納
        /// </summary>
        internal static void SimulationEndStep(
            DataChunk chunk,
            // team
            ref TeamManager.TeamData tdata,
            // collider
            ref NativeArray<float3> nowPositions,
            ref NativeArray<quaternion> nowRotations,
            ref NativeArray<float3> oldPositions,
            ref NativeArray<quaternion> oldRotations
            )
        {
            //int cindex = tdata.colliderChunk.startIndex;
            int cindex = tdata.colliderChunk.startIndex + chunk.startIndex;
            //for (int i = 0; i < tdata.colliderChunk.dataLength; i++, cindex++)
            for (int i = 0; i < chunk.dataLength; i++, cindex++)
            {
                oldPositions[cindex] = nowPositions[cindex];
                oldRotations[cindex] = nowRotations[cindex];
            }
        }

        /// <summary>
        /// シミュレーション更新後処理
        /// </summary>
        internal static void SimulationPostUpdate(
            // team
            ref TeamManager.TeamData tdata,
            // collider
            ref NativeArray<float3> framePositions,
            ref NativeArray<quaternion> frameRotations,
            ref NativeArray<float3> oldFramePositions,
            ref NativeArray<quaternion> oldFrameRotations
            )
        {
            if (tdata.colliderCount == 0)
                return;
            if (tdata.IsRunning == false)
                return;

            int cindex = tdata.colliderChunk.startIndex;
            for (int k = 0; k < tdata.colliderChunk.dataLength; k++, cindex++)
            {
                // コライダー履歴更新
                oldFramePositions[cindex] = framePositions[cindex];
                oldFrameRotations[cindex] = frameRotations[cindex];

            }
        }

        //=========================================================================================
        public void InformationLog(StringBuilder allsb)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"========== Collider Manager ==========");
            if (IsValid() == false)
            {
                sb.AppendLine($"Collider Manager. Invalid.");
            }
            else
            {
                sb.AppendLine($"Collider Manager. Collider:{colliderSet.Count}");
                sb.AppendLine($"  -flagArray:{flagArray.ToSummary()}");
                sb.AppendLine($"  -centerArray:{centerArray.ToSummary()}");
                sb.AppendLine($"  -sizeArray:{sizeArray.ToSummary()}");
                sb.AppendLine($"  -framePositions:{framePositions.ToSummary()}");
                sb.AppendLine($"  -frameRotations:{frameRotations.ToSummary()}");
                sb.AppendLine($"  -frameScales:{frameScales.ToSummary()}");
                sb.AppendLine($"  -oldFramePositions:{oldFramePositions.ToSummary()}");
                sb.AppendLine($"  -oldFrameRotations:{oldFrameRotations.ToSummary()}");
                sb.AppendLine($"  -nowPositions:{nowPositions.ToSummary()}");
                sb.AppendLine($"  -nowRotations:{nowRotations.ToSummary()}");
                sb.AppendLine($"  -oldPositions:{oldPositions.ToSummary()}");
                sb.AppendLine($"  -oldRotations:{oldRotations.ToSummary()}");

                sb.AppendLine($"[Colliders]");
                int cnt = teamIdArray?.Count ?? 0;
                for (int i = 0; i < cnt; i++)
                {
                    var flag = flagArray[i];
                    if (flag.IsSet(Flag_Valid) == false)
                        continue;
                    var ctype = DataUtility.GetColliderType(flag);
                    sb.AppendLine($"  [{i}] tid:{teamIdArray[i]}, flag:0x{flag.Value:X}, type:{ctype}, size:{sizeArray[i]}, cen:{centerArray[i]}");
                }

                sb.AppendLine($"[Collider Names]");
                foreach (var col in colliderSet)
                {
                    var name = col?.name ?? "(null)";
                    sb.AppendLine($"  {name}");
                }
            }
            sb.AppendLine();
            Debug.Log(sb.ToString());
            allsb.Append(sb);
        }
    }
}
