using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class WAgent : Agent {

    public Transform target;
    CharacterController player;

    public float moveSpeed;
    public float rotationSpeed;

    RayPerception rayPer;

    //initialization
    void Start()
    {
        //get the character controller
        player = GetComponent<CharacterController>();
        rayPer = GetComponent<RayPerception>();
    }

    /// <summary>
    /// 重置agent   !!!ATTENTION
    /// 基本逻辑：当agent的位置到达target位置(之间的距离小于一定的阈值时)的时候，随机改变target的位置?移动agent的位置
    /// </summary>
    public override void AgentReset()
    {
        if (Mathf.Abs(this.transform.position.x - target.position.x) < 1f || Mathf.Abs(this.transform.position.z - target.position.z)< 1f)
        {
            //随机改变agent的位置
            this.transform.position = new Vector3(Random.value*400 - 400,
                                                  15,              
                                                  Random.value*200 - 200);
            
        }
    }

    /// <summary>
    /// 收集agent观测信息 TODO:
    /// </summary>
    List<float> observations = new List<float>();
    public override void CollectObservations()
    {
        //使用raycast进行观察,观察标签为wall与obs的障碍
        float raycastDistance = 5f;
        float[] rayAngles = { 20f, 90f, 160f, 45f, 135f, 70f, 110f };
        string[] detectableObjects = { "wall", "target" ,"obs"};
        AddVectorObs(rayPer.Perceive(raycastDistance,rayAngles,detectableObjects,0f,-0.1f));

        //agent的位置(在xoz平面上的位置)并归一化
        AddVectorObs((this.transform.position.x + 40) / (-440));
        AddVectorObs((this.transform.position.z + 40) / 260);

        //agent进行绕y轴旋转
        AddVectorObs(this.transform.rotation.y);
    }
    /// <summary>
    /// 设置奖励，并返回agent动作
    /// </summary>
    private float previousDistance = float.MaxValue;
    public override void AgentAction(float[] vectorAction, string textAction)
    {
        //计算player与target之间的距离
        float distanceToTarget = Vector3.Distance(this.transform.position,target.transform.position);
        if (distanceToTarget < 1.5f)
        {
            //已到达目标,将agent标记为已完成状态
            Done();
            AddReward(1.0f);
        }
        if (distanceToTarget < previousDistance)
        {
            AddReward(0.1f);
        }

        //agent action
        Vector3 dirToGo = Vector3.zero;
        Vector3 rotateDir = Vector3.zero;
        if (brain.brainParameters.vectorActionSpaceType == SpaceType.continuous)
        {
            dirToGo = transform.forward * Mathf.Clamp(vectorAction[0], -1, 1);
            rotateDir = transform.up * Mathf.Clamp(vectorAction[1], -1, 1);
        }
        else
        {
            var forwardAxis = (int)vectorAction[0];
            var rightAxis = (int)vectorAction[1];
            var rotateAxis = (int)vectorAction[2];
            switch (forwardAxis)
            {
                case 1:
                    dirToGo = transform.forward;
                    break;
                case 2:
                    dirToGo = -transform.forward;
                    break;
            }

            switch (rightAxis)
            {
                case 1:
                    dirToGo = transform.right;
                    break;
                case 2:
                    dirToGo = -transform.right;
                    break;
            }
            switch (rotateAxis)
            {
                case 1:
                    rotateDir = -transform.up;
                    break;
                case 2:
                    rotateDir = transform.up;
                    break;
            }
        }

        // Time penalty
        AddReward(-0.005f);
        
    }

    //rayperception使用tag碰撞检测 TODO
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("wall") || collision.gameObject.CompareTag("obs"))
        {
            AddReward(-0.02f);
        }
    }

    public override void AgentOnDone()
    {
        
    }
	
}
