using Ralsei;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Ralsei
{
    public class ParticleBase
    {
        public static Dictionary<string, ParticleSystem> _ParticleSystems = new Dictionary<string, ParticleSystem>();
        public static void InitParticleBase()
        {
            _ParticleSystems = new Dictionary<string, ParticleSystem>()
            {
                {"HeartParticle", UnityEngine.Object.Instantiate(RalseiModule.assetBundle.LoadAsset<GameObject>("RalseiHeartParticle").GetComponent<ParticleSystem>()) },
                {"HealParticle", UnityEngine.Object.Instantiate(RalseiModule.assetBundle.LoadAsset<GameObject>("RalseiHealParticle").GetComponent<ParticleSystem>()) },
            };
            foreach (var item in _ParticleSystems) { UnityEngine.Object.DontDestroyOnLoad(item.Value); }
        }
        public static ParticleSystem ReturnParticleSystem(string name)
        {
            if (!_ParticleSystems.ContainsKey(name))
            {
                return null;
            }
            return _ParticleSystems[name];
        }

        public static void EmitParticles(string name, int amount, ParticleSystem.EmitParams newParams)
        {
            if (!_ParticleSystems.ContainsKey(name))
            {
                return;
            }
            var ParticleSystem = _ParticleSystems[name];
            ParticleSystem.Emit(newParams, amount);
        }

        public static void EmitParticles(string name, int amount, Vector2 position, Vector3 Velocity, float rotation = 0, float lifeTime = 1, float size = 1)
        {
            if (!_ParticleSystems.ContainsKey(name))
            {
                return;
            }
            var newParams = new ParticleSystem.EmitParams();
            newParams.position = position;
            newParams.rotation = rotation;
            newParams.startLifetime = lifeTime;
            newParams.startSize = size;
            newParams.velocity = Velocity;
            var ParticleSystem = _ParticleSystems[name];

            ParticleSystem.Emit(newParams, amount);
        }
        public static void EmitParticles(ParticleSystem particleSystem, int amount, Vector2 position, Vector3 Velocity, float rotation = 0, float lifeTime = 1, float size = 1)
        {
            var newParams = new ParticleSystem.EmitParams();
            newParams.position = position;
            newParams.rotation = rotation;
            newParams.startLifetime = lifeTime;
            newParams.startSize = size;
            newParams.velocity = Velocity;

            particleSystem.Emit(newParams, amount);
        }

        public static void EmitParticles(ParticleSystem particleSystem, int amount, ParticleSystem.EmitParams newParams)
        {
            particleSystem.Emit(newParams, amount);
        }
    }
}
