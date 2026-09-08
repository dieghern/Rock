using System.Collections.Generic;
using UnityEngine;
namespace GetThisRock
{
    public sealed class RockBreaker:MonoBehaviour
    {
        public Rock rock;public float toughness=100;public int generation;float damage;
        public float DamageRatio=>Mathf.Clamp01(damage/toughness);
        public bool Strike(float power,Vector3 point,Vector3 direction)
        {
            if(rock==null)rock=GetComponent<Rock>();var interaction=Object.FindAnyObjectByType<PlayerInteraction>();
            if(rock.mass<=interaction.EffectiveCarryMass*1.05f)return false;
            damage+=Mathf.Max(1,power);rock.integrity=100*(1-DamageRatio);
            rock.GetComponent<Rigidbody>().AddForceAtPosition(direction.normalized*Mathf.Min(power,80)*.08f,point,ForceMode.Impulse);
            if(damage>=toughness)Break();return true;
        }
        void Break()
        {
            var manager=Object.FindAnyObjectByType<ContractManager>();var original=rock;var body=rock.GetComponent<Rigidbody>();float childMass=rock.mass*.5f;
            float diameter=transform.localScale.x*Mathf.Pow(.5f,1f/3f)*.94f;Vector3 axis=Vector3.Cross(Vector3.up,body.linearVelocity.sqrMagnitude>.1f?body.linearVelocity.normalized:transform.right).normalized;if(axis.sqrMagnitude<.1f)axis=transform.right;
            var children=new List<Rock>();
            for(int i=0;i<2;i++)
            {
                var clone=Instantiate(gameObject);clone.name="Fragmento de "+childMass.ToString("0")+" kg";var child=clone.GetComponent<Rock>();child.mass=childMass;child.integrity=100;child.ApplyMass();
                clone.transform.localScale=Vector3.one*diameter;clone.transform.position=transform.position+axis*(i==0?-1:1)*diameter*.54f;
                var breaker=clone.GetComponent<RockBreaker>();breaker.rock=child;breaker.generation=generation+1;breaker.damage=0;breaker.toughness=Mathf.Max(55,toughness*.82f);
                var physical=clone.GetComponent<PhysicalObject>();physical.IsStored=false;physical.allowPickup=childMass<=manager.interaction.EffectiveCarryMass;physical.displayName=physical.allowPickup?"Fragmento pequeño":"Fragmento de roca";
                foreach(var c in clone.GetComponentsInChildren<Collider>())c.enabled=true;var rb=clone.GetComponent<Rigidbody>();rb.isKinematic=false;rb.mass=childMass;rb.linearVelocity=body.linearVelocity;rb.AddForce(axis*(i==0?-1:1)*1.2f,ForceMode.Impulse);
                var visual=clone.GetComponent<RockVisual>();if(visual!=null)visual.Refresh(child.targetId.GetHashCode()+breaker.generation*97+i);children.Add(child);
            }
            manager.ReplaceRock(original,children);gameObject.SetActive(false);if(Application.isPlaying)Destroy(gameObject);else DestroyImmediate(gameObject);
        }
    }
}

