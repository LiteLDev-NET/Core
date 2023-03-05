#pragma once
#include "../Enums/InstanceType.hpp"

namespace LiteLoader::RemoteCall
{
	[System::AttributeUsage(System::AttributeTargets::Class)]
	public ref class RemoteCallValueTypeAttribute : System::Attribute
	{
	public:
		InstanceType type;
		RemoteCallValueTypeAttribute(InstanceType t)
			: type(t)
		{
		}
	};
}