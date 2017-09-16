// ConsoleApplication1.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

struct TypeHandle {};

class ArrayBase
{
// Get the element type for the array, this works whether the element
// type is stored in the array or not
inline TypeHandle GetArrayElementTypeHandle() const;

TypeHandle GetArrayElementTypeHandle()
{
	LIMITED_METHOD_CONTRACT;
	return GetMethodTable()->GetApproxArrayElementTypeHandle();
}

TypeHandle GetApproxArrayElementTypeHandle()
{
	LIMITED_METHOD_DAC_CONTRACT;
	_ASSERTE(IsArray());
	return TypeHandle::FromTAddr(m_ElementTypeHnd);
}

union
{
	PerInstInfo_t m_pPerInstInfo;
	TADDR         m_ElementTypeHnd;
	TADDR         m_pMultipurposeSlot1;
};


	void GetMethodTable() { return null; }

/****************************************************************************/
/* assigns 'val to 'array[idx], after doing all the proper checks */

HCIMPL3(void, JIT_Stelem_Ref_Portable, PtrArray* array, unsigned idx, Object *val)
{
	FCALL_CONTRACT;

	if (!array)
	{
		// ST: explicit check that the array is not null
		FCThrowVoid(kNullReferenceException);
	}
	if (idx >= array->GetNumComponents())
	{
		// ST: bounds check
		FCThrowVoid(kIndexOutOfRangeException);
	}

	if (val)
	{
		MethodTable *valMT = val->GetMethodTable();
		// ST: getting type of an array element
		TypeHandle arrayElemTH = array->GetArrayElementTypeHandle();

		// ST: g_pObjectClass is a pointer to EEClass instance of the System.Object
		// ST: if the element is object than the operation is successful.
		if (arrayElemTH != TypeHandle(valMT) && arrayElemTH != TypeHandle(g_pObjectClass))
		{
			// ST: need to check that the value is compatible with the element type
			TypeHandle::CastResult result = ObjIsInstanceOfNoGC(val, arrayElemTH);
			if (result != TypeHandle::CanCast)
			{
				// ST: ArrayStoreCheck throws ArrayTypeMismatchException if the types are incompatible
				if (HCCALL2(ArrayStoreCheck, (Object**)&val, (PtrArray**)&array) != NULL)
				{
					return;
				}
			}
		}

		HCCALL2(JIT_WriteBarrier, (Object **)&array->m_Array[idx], val);
	}
	else
	{
		// no need to go through write-barrier for NULL
		ClearObjectReference(&array->m_Array[idx]);
	}
}


};


int main()
{
    return 0;
}

